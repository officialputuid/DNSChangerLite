using System;
using System.Diagnostics;
using System.Management;
using System.Collections.Generic;
using System.Linq;

namespace DnsChangerLite
{
    /// <summary>
    /// Network adapter info for DNS operations.
    /// </summary>
    internal sealed class NetworkAdapter
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string DisplayName => string.IsNullOrEmpty(Description)
            ? Name
            : $"{Name} — {Description}";

        public override string ToString() => DisplayName;
    }

    /// <summary>
    /// DNS configuration result.
    /// </summary>
    internal sealed class DnsResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
    }

    /// <summary>
    /// Handles DNS operations via netsh.exe and WMI.
    /// All netsh paths resolved from SystemDirectory to prevent binary planting.
    /// </summary>
    internal sealed class DnsService
    {
        private static readonly string NetshPath =
            System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "netsh.exe");

        private static readonly string IpconfigPath =
            System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "ipconfig.exe");

        /// <summary>
        /// Enumerate enabled network adapters with IP enabled.
        /// </summary>
        public List<NetworkAdapter> GetAdapters()
        {
            var adapters = new List<NetworkAdapter>();
            try
            {
                using (var searcher = new ManagementObjectSearcher(
                    "SELECT Name, Description, NetConnectionID, NetConnectionStatus " +
                    "FROM Win32_NetworkAdapter WHERE NetConnectionStatus = 2 AND PhysicalAdapter = True"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        var name = obj["NetConnectionID"] as string;
                        if (string.IsNullOrEmpty(name)) continue;

                        adapters.Add(new NetworkAdapter
                        {
                            Name = name,
                            Description = obj["Description"] as string ?? ""
                        });
                    }
                }
            }
            catch
            {
                // WMI not available; fall back to empty list
            }
            return adapters;
        }

        /// <summary>
        /// Get current DNS servers for an adapter via WMI.
        /// Matches adapter by Description against Win32_NetworkAdapterConfiguration.
        /// </summary>
        public string GetCurrentDns(string adapterDescription)
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher(
                    "SELECT Description, DNSServerSearchOrder, DHCPEnabled " +
                    "FROM Win32_NetworkAdapterConfiguration WHERE IPEnabled = True"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        var desc = obj["Description"] as string ?? "";
                        if (!desc.Equals(adapterDescription, StringComparison.OrdinalIgnoreCase))
                            continue;

                        var servers = obj["DNSServerSearchOrder"] as string[];
                        if (servers == null || servers.Length == 0)
                            return "DHCP (Automatic)";

                        // Filter out empty entries
                        var valid = servers.Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
                        if (valid.Length == 0)
                            return "DHCP (Automatic)";

                        return string.Join(", ", valid);
                    }
                }
            }
            catch { }
            return "Unknown";
        }

        /// <summary>
        /// Set static DNS servers on an adapter.
        /// </summary>
        public DnsResult SetDns(string adapterName, string primaryDns, string secondaryDns)
        {
            if (string.IsNullOrWhiteSpace(adapterName))
                return new DnsResult { Success = false, Message = "No adapter selected." };

            // Set primary DNS
            var p1 = RunNetsh($"interface ipv4 set dnsservers name=\"{adapterName}\" static {primaryDns} primary validate=no");
            if (p1.Contains("requires elevation") || p1.Contains("Access is denied"))
                return new DnsResult { Success = false, Message = "Run as Administrator required." };
            if (!p1.Contains("Ok") && p1.Length > 0 && !p1.Contains("done"))
                return new DnsResult { Success = false, Message = $"Primary DNS failed: {p1.Trim()}" };

            // Set secondary DNS
            if (!string.IsNullOrWhiteSpace(secondaryDns))
            {
                var p2 = RunNetsh($"interface ipv4 add dnsservers name=\"{adapterName}\" {secondaryDns} index=2 validate=no");
                if (!p2.Contains("Ok") && p2.Length > 0 && !p2.Contains("done"))
                    return new DnsResult { Success = true, Message = $"Primary OK, secondary failed: {p2.Trim()}" };
            }

            return new DnsResult { Success = true, Message = $"DNS set to {primaryDns}" +
                (string.IsNullOrWhiteSpace(secondaryDns) ? "" : $", {secondaryDns}") };
        }

        /// <summary>
        /// Reset DNS to DHCP (automatic).
        /// </summary>
        public DnsResult ResetDns(string adapterName)
        {
            if (string.IsNullOrWhiteSpace(adapterName))
                return new DnsResult { Success = false, Message = "No adapter selected." };

            var output = RunNetsh($"interface ipv4 set dnsservers name=\"{adapterName}\" dhcp");
            if (output.Contains("requires elevation") || output.Contains("Access is denied"))
                return new DnsResult { Success = false, Message = "Run as Administrator required." };

            return new DnsResult { Success = true, Message = "DNS reset to automatic (DHCP)." };
        }

        /// <summary>
        /// Flush DNS resolver cache.
        /// </summary>
        public DnsResult FlushDns()
        {
            var output = RunIpconfig("/flushdns");
            if (output.Contains("Successfully flushed"))
                return new DnsResult { Success = true, Message = "DNS cache flushed." };
            return new DnsResult { Success = false, Message = $"Flush failed: {output.Trim()}" };
        }

        private static string RunNetsh(string arguments)
        {
            return RunProcess(NetshPath, arguments);
        }

        private static string RunIpconfig(string arguments)
        {
            return RunProcess(IpconfigPath, arguments);
        }

        private static string RunProcess(string fileName, string arguments)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(psi))
                {
                    var stdout = process.StandardOutput.ReadToEnd();
                    var stderr = process.StandardError.ReadToEnd();
                    process.WaitForExit(10000);

                    if (!string.IsNullOrWhiteSpace(stderr))
                        return stderr.Trim();
                    return stdout.Trim();
                }
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }
    }
}

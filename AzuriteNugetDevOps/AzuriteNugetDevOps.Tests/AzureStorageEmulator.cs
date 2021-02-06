using System.Diagnostics;

namespace AzuriteNugetDevOps.Tests
{
    internal static class AzureStorageEmulator
    {
        public static Process DockerProcess => new Process
        {
            StartInfo =
            {
                UseShellExecute = false,
                FileName = "docker"
            }
        };

        public static void StartEmulator(this Process process)
            => process.Execute("run -p 10000:10000 mcr.microsoft.com/azure-storage/azurite azurite-blob --blobHost 0.0.0.0");

        public static void StopEmulator(this Process process)
            => process.Execute("container stop $(docker container ls -q --filter ancestor=mcr.microsoft.com/azure-storage/azurite)");

        private static void Execute(this Process process, string command)
        {
            process.StartInfo.Arguments = command;
            process.Start();
            process.WaitForExit(10000);
        }
    }
}

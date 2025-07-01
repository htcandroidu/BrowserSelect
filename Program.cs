using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace BrowserSelect;

static class RegistryExtension
{
    public static RegistryKey? CreateOrOpenSubKey(this RegistryKey r, string name, bool writeable)
    {
        if (r.GetSubKeyNames().Contains(name) == false)
        {
            return r.CreateSubKey(name, writeable);
        }
        else
        {
            return r.OpenSubKey(name, writeable);
        }
    }
}

class Program
{
    [STAThread]
    public static void Main2(string[] args)
    {
        if (args.Any() && args.Contains("--install"))
        {
            RegistryKey? r0, r1, r2, r3, r4, r5, r6, r7, r8, r9, r10, r11, r12, r13, r14, r15, r16, r17, r18;

            r0 = r1 = r2 = r3 = r4 = r5 = r6 = r7 = r8 = r9 = r10 = r11 = r12 = r13 = r14 = r15 = r16 = r17 = r18 = null;

            try
            {
                r0 = Registry.LocalMachine.CreateOrOpenSubKey(@"SOFTWARE\RegisteredApplications", true);

                r0?.SetValue("kenaro.BrowserSelect", @"Software\kenaro\kenaro.BrowserSelect\Capabilities");
            }
            finally
            {
                r0?.Close();
            }

            try
            {
                r1 = Registry.LocalMachine.CreateOrOpenSubKey(@"SOFTWARE", true);
                try
                {
                    r2 = r1?.CreateOrOpenSubKey(@"kenaro", true);
                    try
                    {
                        r3 = r2?.CreateOrOpenSubKey(@"kenaro.BrowserSelect", true);
                        try
                        {
                            r4 = r3?.CreateOrOpenSubKey(@"Capabilities", true);
                            r4?.SetValue("ApplicationName", "kenaro BrowserSelect");
                            r4?.SetValue("ApplicationDescription", "Select your preferred browser and browser profile for opening links or just starting the browser with the required profile.");

                            try
                            {
                                r5 = r4?.OpenSubKey(@"Startmenu", true);

                                r5?.SetValue("StartmenuInternet", $@"""{Path.ChangeExtension(typeof(Program).Assembly.Location, ".exe")}""");
                            }
                            finally
                            {
                                r5?.Close();
                            }

                            try
                            {
                                r6 = r4?.OpenSubKey(@"UrlAssociations", true);

                                r6?.SetValue("http", "kenaro.BrowserSelect");
                                r6?.SetValue("https", "kenaro.BrowserSelect");

                                r6.Close();
                            }
                            finally { r6?.Close(); }

                        }
                        finally { r4?.Close(); }

                        try
                        {
                            r7 = r3?.OpenSubKey(@"shell", true);
                            try
                            {
                                r8 = r7?.OpenSubKey(@"Open", true);
                                try
                                {
                                    r9 = r8?.OpenSubKey(@"Command", true);

                                    r9?.SetValue("", Path.ChangeExtension(typeof(Program).Assembly.Location, ".exe"));

                                }
                                finally { r9?.Close(); }
                            }
                            finally { r8?.Close(); }
                        }
                        finally { r7?.Close(); }
                    }
                    finally { r3?.Close(); }
                }
                finally { r2?.Close(); }

                try
                {
                    r10 = r1?.CreateOrOpenSubKey("Classes", true);

                    try
                    {
                        r11 = r10?.CreateOrOpenSubKey(@"kenaro.BrowserSelect", true);

                        try
                        {
                            r12 = r11?.CreateOrOpenSubKey(@"Application", true);

                            r12?.SetValue("ApplicationCompany", "kenaro");
                            r12?.SetValue("ApplicationDescription", "Select your preferred browser and browser profile for opening links or just starting the browser with the required profile.");
                            r12?.SetValue("ApplicationName", "kenaro BrowserSelect");
                            r12?.SetValue("ApplicationIcon", $@"{Path.ChangeExtension(typeof(Program).Assembly.Location, ".exe")},0");
                        }
                        finally { r12?.Close(); }

                        try
                        {
                            r13 = r11?.CreateOrOpenSubKey(@"DefaultIcon", true);

                            r13?.SetValue("", $@"{typeof(Program).Assembly.Location},0");
                        }
                        finally { r13?.Close(); }

                        try
                        {
                            r14 = r11?.CreateOrOpenSubKey(@"shell", true);

                            try
                            {
                                r15 = r14?.CreateOrOpenSubKey(@"Open", true);

                                try
                                {
                                    r16 = r15?.CreateOrOpenSubKey(@"Command", true);

                                    r16?.SetValue("", $@"""{Path.ChangeExtension(typeof(Program).Assembly.Location, ".exe")}"" ""%1""");
                                }
                                finally { r16?.Close(); }
                            }
                            finally { r15?.Close(); }
                        }
                        finally { r14?.Close(); }
                    }
                    finally { r11?.Close(); }
                }
                finally { r10?.Close(); }
            }
            finally { r1?.Close(); }


            return;
        }

        var application = new App();
        application.Run();
    }
}

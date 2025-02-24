﻿using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Security.Cryptography; //MD5, also yes no PwnieCastle or BouncyCastle is actually used.
using System.Net;

//INI library
using IniParser;
using IniParser.Model;
using System.Xml.Linq;
using System.Collections.Generic;
using System.Text;

using System.Linq;

// le l10n
using System.Globalization;
using System.Resources;

namespace RussellNX
{
    public partial class MainForm : Form
    {
        public static string AppDir = AppDomain.CurrentDomain.BaseDirectory;
        public static string RuntimeVerString = string.Empty;

        // mhmm...... can we somehow make it Windows & macOS friendly instead of Windows only?
        public static string RuntimePath = string.Empty;
        public static string FriendlyYYPName = string.Empty;
        public static string GameIconPath = AppDir + "default_icon.jpg";
        public static string RNXVersionString = "1.6.3";

        public MainForm()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
        }

        private void CleanupTempDirs()
        {
            //Cleanup temp dirs
            try
            {
                for (int i = 1000; i <= 9999; i++)
                {
                    if (Directory.Exists(AppDir + "TEMPDIR" + i.ToString()))
                    {
                        try
                        {
                            Directory.Delete(AppDir + "TEMPDIR" + i.ToString(), true);
                        }
                        catch { }
                    }
                }
            }
            catch { }
        }

        private void CheckForKeys(string ci)
        {

            //Check for keys.txt here
            if (!File.Exists(KeysBox.Text))
            {
                string keyStr = "Please specify your Switch keys file after clicking OK";
                if (ci == "ru-RU") keyStr = "Пожалуйста выберите файл с ключами от свитча после нажатия ОК";
                MessageBox.Show(keyStr, "No keys file specified!", MessageBoxButtons.OK, MessageBoxIcon.Information);
                OpenFileDialog KeysChooseDialog = new OpenFileDialog();
                KeysChooseDialog.Filter = "All Files|*.*";

                if (KeysChooseDialog.ShowDialog() == DialogResult.OK)
                {
                    KeysBox.Text = KeysChooseDialog.FileName;
                }
            }
        }

        private void RandomQuotePrint(string ci)
        {
            // hehe.
            var rnd = new Random().NextDouble();
            if (rnd >= 0.5)
            {
                if (ci == "ru-RU")
                    prnt("Чё пацаны, могилизация?");
                else
                    prnt("Finally with .NET 6 runtime support!");
            }

            if (Debugger.IsAttached) Text += " (Running inside Visual Studio)";
        }

        private void IconChooseBtn_Click(object sender, EventArgs e)
        {
            var ci = CultureInfo.CurrentUICulture.Name;
            string iconStr = "Your icon must be a JPEG (.jpg) 256x256 image!";
            if (ci == "ru-RU") iconStr = "Ваша иконка должна быть ЖЫПЕГ картинкой с разрешением 256 на 256!";

            MessageBox.Show(iconStr, "Icon format message", MessageBoxButtons.OK, MessageBoxIcon.Information);

            OpenFileDialog IconChooseDialog = new OpenFileDialog();
            IconChooseDialog.Filter = "JPEG Icon (*.jpg)|*.jpg";

            if (IconChooseDialog.ShowDialog() == DialogResult.OK)
            {
                Bitmap NXAppIcon = new Bitmap(IconChooseDialog.FileName);
                var NXAppIconW = NXAppIcon.Width;
                var NXAppIconH = NXAppIcon.Height;

                //Icon MUST BE 256x256 c'mon!
                if ((NXAppIconW != 256) || (NXAppIconH != 256))
                {
                    MessageBox.Show("Your icon size is invalid!\nIt should be 256x256", "Error in loading icon.", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    IconPicBox.Image = NXAppIcon;
                    GameIconPath = IconChooseDialog.FileName;
                }
            }
        }

        private void OpenProjectBtn_Click(object sender, EventArgs e)
        {
            OpenFileDialog ProjectChooseDialog = new OpenFileDialog();
            ProjectChooseDialog.Filter = "GMS2 Project (*.yyp)|*.yyp";

            if (ProjectChooseDialog.ShowDialog() == DialogResult.OK)
            {
                ProjectPathBox.Text = ProjectChooseDialog.FileName;
                FriendlyYYPName = Path.GetFileNameWithoutExtension(ProjectChooseDialog.FileName);
                prnt("GameName: " + FriendlyYYPName);
                //MessageBox.Show(FriendlyYYPName);
            }

            ProjectChooseDialog.Dispose();
        }


        //Thanks Microsoft for this function! Very helpful :33
        private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();
            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, false);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                }
            }
        }

        private void ApplyLocalisation()
        {
            var ci = CultureInfo.CurrentUICulture.Name;
            if (ci == "ru-RU")
            {
                prnt("Применяю русские строки...");

                IconLabel.Text = "Иконка (жыпег):";
                IconChooseBtn.Text = "Выбрать другую иконку...";
                RuntimeLabelBox.Text = "Рантайм:";
                AdvancedOptionsLabel.Text = "Расширенные настройки:";
                StartupAccCheckbox.Text = "Требовать выбора аккаунта перед запуском игры?";
                DataLossCheckbox.Text = "Показывать окно о возможной потере данных перед выходом?";
                LanguagesLabel.Text = "Поддерживаемые языки:";
                IncludeYYdebugCheckbox.Text = "Оставить .yydebug? (строки)";

                /*
                aengCheckbox.Text = "Американский Английский";
                freCheckbox.Text = "Французский";
                spaCheckbox.Text = "Испанский";
                itaCheckbox.Text = "Итальянский";
                rusCheckbox.Text = "Русский";
                dutCheckbox.Text = "Нидерландский";
                porCheckbox.Text = "Португальский";
                gerCheckbox.Text = "Немецкий";
                */

                LogTitle.Text = "Лог:";
                ProjectPathLabel.Text = "Файл проекта:";
                GameNameLabel.Text = "Имя игры:";
                AuthorLabel.Text = "Автор(ы) игры:";
                VersionLabel.Text = "Версия игры:";
                KeysLabel.Text = "Путь к ключам шифрования сыча (prod.keys, keys.dat, keys.txt):";
                ProjectSettingsBtn.Text = "Настройки проекта...";
                RNXVersionLabel.Text = "Версия RussellNX: 0.0.0";
                this.Text = "RussellNX : Главное меню.";
                BuildButton.Text = "Собрать .NSP!";
                CleanLogBtn.Text = "Очистить лог";
                ExportLogBtn.Text = "Сохранить лог";
                ProjectConfigLabel.Text = "Имя конфигурации:";
            }
        }

        private void prnt(string log)
        {
            //Special command that cleans the LogBox.
            if (log == "$LOG_CLEAN") LogBox.Clear();
            else LogBox.AppendText(log + "\n");
        }

        private bool CheckTitleID()
        {
            string tt = TitleIDBox.Text;
            ulong dummy = 0;
            if (tt.Length < TitleIDBox.MaxLength || !UInt64.TryParse(tt, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out dummy))
            {
                MessageBox.Show("Invalid title ID!", "ERROR!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            return true;
        }

        private void BuildButton_Click(object sender, EventArgs e)
        {
            var isnet6runtime = false;
            //Idiot checks. (a lot of)
            if (!File.Exists(ProjectPathBox.Text))
            {
                MessageBox.Show("Project file does not exist!\nPlease select a valid project file.", "ERROR!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!File.Exists(GameIconPath))
            {
                MessageBox.Show("Icon does not exist!\nPlease select your icon again.", "ERROR!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!File.Exists(KeysBox.Text))
            {
                MessageBox.Show("keys.txt does not exist!\nPlease select your keys.txt file again.", "ERROR!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!File.Exists(Path.Combine(RuntimePath, "bin", "GMAssetCompiler.exe")))
            {
                if (File.Exists(Path.Combine(RuntimePath, "bin", "assetcompiler", "windows", "x64", "GMAssetCompiler.exe")))
                {
                    prnt("Using .NET 6+ / 2022.6+ runtime");
                    isnet6runtime = true;
                }
                else
                {
                    MessageBox.Show("This path is invalid, maybe your runtime version is invalid?\n\nrpath: " + RuntimePath, "ERROR!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                
            }

            if (!File.Exists(Path.Combine(AppDir, "license", "licence.plist")))
            {
                MessageBox.Show("Couldn't find fakesigned licence.plist file.\nPlease redownload RussellNX.", "ERROR!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string prebuiltPath = Path.Combine(AppDir, "runners", "build" + RuntimeVerString);
            if (!Directory.Exists(prebuiltPath))
            {
                MessageBox.Show("ERROR! Nik didn't built an ExeFS for your runtime version,\ncould you please try a different one?\n\n(or contact nik at nik#5351 and tell him the version you want)", "ERROR!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!CheckTitleID()) return;

            // this is going to get patched soon ;-;
            // screw whoever reported this >:(
            if (isnet6runtime || ((RuntimeVersion)RuntimeChooserBox.SelectedItem).IsNuBeta || (int.Parse(RuntimeVerString.Substring(4, 1)) > 3)) // if third integer in runtime version is >3 (newer than 2.2.3)
            {
                string hash = "";
                string pwniehash = "";

                if (!File.Exists(AppDir + "PwnieCastle.Crypto.dll"))
                {
                    prnt("ERROR! PwnieCastle library doesn't exist, redownload RussellNX! Building aborted!");
                    return;
                }

                var bcdllpath = isnet6runtime
                        ? (RuntimePath + "\\bin\\assetcompiler\\windows\\x64\\BouncyCastle.Crypto.dll")
                        : (RuntimePath + "\\bin\\BouncyCastle.Crypto.dll");
                var bcbakpath = isnet6runtime
                        ? (RuntimePath + "\\bin\\assetcompiler\\windows\\x64\\BouncyCastle.Crypto.bak")
                        : (RuntimePath + "\\bin\\BouncyCastle.Crypto.bak");

                using (var md5 = MD5.Create())
                {
                    using (var stream = File.OpenRead(bcdllpath))
                    {
                        hash = BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "").ToUpperInvariant();
                    }

                    using (var stream = File.OpenRead(AppDir + "PwnieCastle.Crypto.dll"))
                    {
                        pwniehash = BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "").ToUpperInvariant();
                    }
                }

                prnt("hash of BouncyCastle.Crypto.dll: " + hash);
                if (hash != pwniehash) //hash of (new) PwnieCastle.Crypto.Dll
                {
                    DialogResult result = MessageBox.Show("Hey, your runtime is not vulnerable to public_key\nAnd it doesn't have PwnieCastle installed, would you like to copy over PwnieCastle.Crypto.dll from RussellNX directory to bypass /m=switch license check?", "PwnieCastle Message", MessageBoxButtons.YesNo);
                    if (result == DialogResult.No)
                    {
                        prnt("User aborted installation of PwnieCastle.Crypto.dll, cannot proceed!");
                        return;
                    }
                    else if (result == DialogResult.Yes)
                    {
                        prnt("Installing PwnieCastle.Crypto.dll...");
                        string bkpath = bcbakpath;
                        if (!File.Exists(bkpath))
                            File.Copy(bcdllpath, bcbakpath);
                        File.Copy(AppDir + @"PwnieCastle.Crypto.dll", bcdllpath, true);
                        prnt("Done! Backup is saved in " + bcbakpath);
                    }
                    else prnt("?????????? wtf dialog result?? try again, please.");
                }
            }

            BuildButton.Enabled = false;
            SaveSettings();

            //Let the build begin >:)
            prnt("$LOG_CLEAN"); //clean LogBox

            prnt("BUILD BEGIN:");

            //Make a temp build directory.
            prnt("Making Temp Directory");
            Random ind = new Random();
            int ind2 = ind.Next(1000, 9999);
            string TempDirectoryPath = AppDir + "TEMPDIR" + ind2.ToString();
            if (Directory.Exists(TempDirectoryPath)) Directory.Delete(TempDirectoryPath);
            Directory.CreateDirectory(TempDirectoryPath);

            //Put some files...
            prnt("Copying stuff");
            DirectoryCopy(prebuiltPath, TempDirectoryPath + "\\build", true);
            prnt("Copying your icon as multiple for each language... :p");
            string iconsDir = TempDirectoryPath + "\\build\\control\\";
            Directory.CreateDirectory(iconsDir);
            CopyLang(aengCheckbox, iconsDir); //always enabled...
            CopyLang(freCheckbox, iconsDir);
            CopyLang(spaCheckbox, iconsDir);
            CopyLang(itaCheckbox, iconsDir);
            CopyLang(rusCheckbox, iconsDir);
            CopyLang(dutCheckbox, iconsDir);
            CopyLang(porCheckbox, iconsDir);
            CopyLang(gerCheckbox, iconsDir);

            prnt("Generating GMAssetCompiler args str");
            string GMACPath = isnet6runtime
                ? Path.Combine(RuntimePath, "bin", "assetcompiler", "windows", "x64", "GMAssetCompiler.exe")
                : Path.Combine(RuntimePath, "bin", "GMAssetCompiler.exe");
            string BaseProjPath = Path.Combine(RuntimePath, "BaseProject", "BaseProject.yyp");
            string GameProjPath = ProjectPathBox.Text;
            string GameName = FriendlyYYPName;
            string CacheDir = TempDirectoryPath + "\\CelesteCacheDir" + (ind2 - 10).ToString();
            string TempDir = TempDirectoryPath + "\\CelesteTempDir" + (ind2 + 10).ToString();
            string OutputDir = TempDirectoryPath + "\\build\\romfs";
            string INIDir = TempDirectoryPath + "\\build\\romfs\\options.ini";

            string LicensePlistPath = AppDir + "license"; //public_key'd already ;)

            //shader fix, fuck yoyo
            Directory.CreateDirectory(TempDir);
            Directory.CreateDirectory(CacheDir);
            Directory.CreateDirectory(OutputDir);

            // pwniecastle.crypto fix:
            string TxtPath = isnet6runtime
                ? Path.Combine(RuntimePath, "bin", "assetcompiler", "windows", "x64", "RussellNXFlag.txt")
                : Path.Combine(RuntimePath, "bin", "RussellNXFlag.txt");
            try
            {
                prnt("Creating flag file at path " + TxtPath);
                File.WriteAllText(TxtPath, "; This file is used to tell BouncyCastle.Crypto.dll to swap the public_key when needed.");
            }
            catch
            {
                prnt("Failed to make a flag file, ignoring...");
            }

            bool hasYydebug = IncludeYYdebugCheckbox.Checked;
            if (hasYydebug)
            {
                prnt("[RUSSELLNX|DEBUG WARNING]: Will include the game.yydebug file and the debug output lib.");
            }
            else
            {
                try
                {
                    File.Delete(Path.Combine(OutputDir, "nro", "YYSwitchOutputLib.nro"));
                    File.Delete(Path.Combine(OutputDir, ".nrr", "YYSwitchOutputLib.nrs"));
                    File.Delete(Path.Combine(OutputDir, ".nrr", "YYSwitchOutputLib.nrr"));
                }
                catch
                {
                    prnt("[RUSSELLNX|WARNING]: Failed to exclude the debug output lib.");
                }
            }
            // true  => /bt=run - running game thru TM (but we build an NSP anyway ;p)
            // false => /bt=exe - building package file (NSP)
            string btarg = hasYydebug ? "run" : "exe";

            // so preselected will actually work correctly this time!
            string preselectedflag = StartupAccCheckbox.Checked.ToString(CultureInfo.InvariantCulture);
            prnt("Writing preselected user flag to " + preselectedflag);
            var pspath = Path.Combine(OutputDir, "preselected_user");
            File.WriteAllText(pspath, preselectedflag);

            string GMACArgs = @" /c /v /zpex /mv=1 /iv=0 /rv=0 /bv=0 /j=9 /gn=""" + GameName + @""" /td=""" + TempDir + @""" /cd=""" + CacheDir + @""" /rtp=""" + RuntimePath + @""" /zpuf=""" + LicensePlistPath + @"""" + @" /ffe=""eXpvfGtxgjeDg202c3h+b3Z2c31veH1vNnh/dnZzfXI2dnlxc3hpfX15Nn5vfX4="" " + " /m=switch /tgt=144115188075855872 /cvm /bt=" + btarg + @" /rt=vm /sh=True /nodnd /cfg=""" + ProjectConfigBox.Text + @""" /o=""" + OutputDir + @""" /optionsini=""" + INIDir + @""" /baseproject=""" + BaseProjPath + @""" " + @"""" + GameProjPath + @""" /v /preprocess=""" + CacheDir + @"""";
            prnt(GMACArgs);
            //return;

            string args = "";

            //Compile game
            
            prnt("\nPreprocessing game project...\n");

            Process process = new Process();
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.WorkingDirectory = RuntimePath + "\\bin";
            process.StartInfo.FileName = GMACPath;
            process.StartInfo.Arguments = GMACArgs;
            process.ErrorDataReceived += (a, b) => { prnt(b.Data); };
            process.OutputDataReceived += (a, b) => { prnt(b.Data); };
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            while (!process.HasExited) Application.DoEvents(); 
            process.WaitForExit();
            process.CancelErrorRead();
            process.CancelOutputRead();

            if (process.ExitCode != 0)
            {
                prnt("GMAC exited with exit code " + process.ExitCode + " durnig preprocess, there were errors...");
                return;
            }

            GMACArgs = @" /c /v /zpex /mv=1 /iv=0 /rv=0 /bv=0 /j=9 /gn=""" + GameName + @""" /td=""" + TempDir + @""" /cd=""" + CacheDir + @""" /rtp=""" + RuntimePath + @""" /zpuf=""" + LicensePlistPath + @"""" + @" /ffe=""eXpvfGtxgjeDg202c3h+b3Z2c31veH1vNnh/dnZzfXI2dnlxc3hpfX15Nn5vfX4=""" + " /m=switch /tgt=144115188075855872 /cvm /bt=" + btarg + @" /rt=vm /sh=True /nodnd /cfg=""" + ProjectConfigBox.Text + @""" /o=""" + OutputDir + @""" /optionsini=""" + INIDir + @""" /baseproject=""" + BaseProjPath + @""" " + @"""" + GameProjPath + @""" /v ";
            process.StartInfo.Arguments = GMACArgs;
            prnt(GMACArgs);

            prnt("\nBuilding your project...\n");
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            while (!process.HasExited) Application.DoEvents();
            process.WaitForExit();
            process.CancelErrorRead();
            process.CancelOutputRead();

            if (process.ExitCode != 0)
            {
                prnt("GMAC exited with exit code " + process.ExitCode + " durnig compilation, there were errors...");
                return;
            }

            try
            {
                prnt("\nDeleting flag file...\n");
                File.Delete(TxtPath);
            }
            catch
            {
                prnt("\nFailed to delete the flag, ignoring...\n");
            }

            prnt("\nGenerating control.nacp...");
            XDocument xml = XDocument.Load(AppDir + "runners\\dummy.xml");
            List<string> EnabledLangs = new List<string>();
            if (aengCheckbox.Checked) EnabledLangs.Add(aengCheckbox.Text);
            if (freCheckbox.Checked) EnabledLangs.Add(freCheckbox.Text);
            if (spaCheckbox.Checked) EnabledLangs.Add(spaCheckbox.Text);
            if (itaCheckbox.Checked) EnabledLangs.Add(itaCheckbox.Text);
            if (rusCheckbox.Checked) EnabledLangs.Add(rusCheckbox.Text);
            if (dutCheckbox.Checked) EnabledLangs.Add(dutCheckbox.Text);
            if (porCheckbox.Checked) EnabledLangs.Add(porCheckbox.Text);
            if (gerCheckbox.Checked) EnabledLangs.Add(gerCheckbox.Text);

            foreach (string lang in EnabledLangs)
            {
                xml.Element("Application").Add(new XElement("SupportedLanguage", lang));
                xml.Element("Application").Add(new XElement("Title", new XElement("Language", lang), new XElement("Name", GameNameBox.Text), new XElement("Publisher", AuthorBox.Text)));
            }

            xml.Element("Application").Element("DisplayVersion").Value = VersionBox.Text;
            xml.Element("Application").Element("DataLossConfirmation").Value = DataLossCheckbox.Checked ? "Required" : "None";
            xml.Element("Application").Element("StartupUserAccount").Value = StartupAccCheckbox.Checked ? "Required" : "None";

            //titleid stuff
            xml.Element("Application").Element("SaveDataOwnerId").Value = "0x" + TitleIDBox.Text.ToLower();
            xml.Element("Application").Element("PresenceGroupId").Value = "0x" + TitleIDBox.Text.ToLower();
            xml.Element("Application").Element("AddOnContentBaseId").Value = "0x" + TitleIDBox.Text.ToLower();
            xml.Element("Application").Element("LocalCommunicationId").Value = "0x" + TitleIDBox.Text.ToLower();
            xml.Element("Application").Element("SeedForPseudoDeviceId").Value = "0x" + TitleIDBox.Text.ToLower();

            xml.Save(TempDirectoryPath + "\\temp.xml");

            process.StartInfo.FileName = AppDir + "hptnacp.exe";
            process.StartInfo.WorkingDirectory = TempDirectoryPath;
            process.StartInfo.Arguments = @"-a createnacp -i temp.xml -o .\build\control\control.nacp";
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            while (!process.HasExited) Application.DoEvents();
            process.WaitForExit();
            process.CancelErrorRead();
            process.CancelOutputRead();

            prnt("Patching main.npdm...");
            string npdmPath = TempDirectoryPath + "\\build\\exefs\\main.npdm";
            byte[] npdmData = File.ReadAllBytes(npdmPath);
            //1104 offset to titleID.
            //1111 end. (last byte)
            //8 bytes.
            int d = 0;
            for (int c = 0; c < 8; c++)
            {
                prnt("byte " + c.ToString());
                string curChar = TitleIDBox.Text.Substring(d, 2);
                byte curByte = Convert.ToByte(curChar, 16);
                npdmData[1111 - c] = curByte;
                d += 2;
            }
            File.Delete(npdmPath);
            File.WriteAllBytes(npdmPath, npdmData);
            //TODO!!

            //MessageBox.Show("break on me");

            prnt("\nBuilding NSP...");

            var exefsdir = @".\build\exefs";
            var romfsdir = @".\build\romfs";
            var logodir = @".\build\logo";
            var controldir = @".\build\control";
            args = @" -k """ + KeysBox.Text + @""" --keygeneration 5 --exefsdir " + exefsdir + @" --romfsdir " + romfsdir + @" --logodir " + logodir + @" --controldir " + controldir + @" --nopatchnacplogo --sdkversion 09030209";
            prnt(args);
            process.StartInfo.Arguments = args;
            process.StartInfo.FileName = AppDir + "hacbrewpack.exe";
            process.StartInfo.WorkingDirectory = TempDirectoryPath;
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            while (!process.HasExited) Application.DoEvents();
            process.WaitForExit();
            process.CancelErrorRead();
            process.CancelOutputRead();

            //free things
            process.Close();
            process.Dispose();

            prnt("\nDone!");
            prnt("Your NSP is at " + TempDirectoryPath + "\\hacbrewpack_nsp");
            Process.Start("explorer.exe", TempDirectoryPath + "\\hacbrewpack_nsp"); //open the build directory.
            prnt("Explorer window with your file should be opened...");
            prnt("Thanks for using RussellNX and god bless the United States of France!");

            BuildButton.Enabled = true;
        }

        private void KeysBtn_Click(object sender, EventArgs e)
        {
            OpenFileDialog KeysChooseDialog = new OpenFileDialog();
            KeysChooseDialog.Filter = "All Files|*.*";

            if (KeysChooseDialog.ShowDialog() == DialogResult.OK)
            {
                KeysBox.Text = KeysChooseDialog.FileName;
            }

            KeysChooseDialog.Dispose();
        }

        private void LoadSettings()
        {
            var parser = new FileIniDataParser();
            IniData data = parser.ReadFile(AppDir + "RussellNX.ini");
            ProjectPathBox.Text = data["Main"]["AppYYPPath"];
            TitleIDBox.Text = data["Main"]["AppID"];
            GameNameBox.Text = data["Main"]["AppName"];
            AuthorBox.Text = data["Main"]["AppAuthor"];
            VersionBox.Text = data["Main"]["AppVersion"];
            KeysBox.Text = data["Main"]["AppKeysPath"];
            GameIconPath = data["Main"]["AppIconPath"];
            RuntimeVerString = data["Main"]["RuntimeVersion"];
            if (data["Main"].ContainsKey("ConfigurationName"))
            {
                ProjectConfigBox.Text = data["Main"]["ConfigurationName"];
            }

            if (data["Main"].ContainsKey("IncludeYYdebug"))
            {
                IncludeYYdebugCheckbox.Checked = data["Main"]["IncludeYYdebug"] == "Yes";
            }

            bool found = false;

            foreach (RuntimeVersion item in RuntimeChooserBox.Items)
            {
                if (item.Version == RuntimeVerString)
                {
                    RuntimeChooserBox.SelectedItem = item;
                    found = true;
                    break;
                }
            }

            if (found) RuntimePath = ((RuntimeVersion)RuntimeChooserBox.SelectedItem).FullPath;
            FriendlyYYPName = Path.GetFileNameWithoutExtension(ProjectPathBox.Text);
            LoadCheckboxStr(data["Main"]["CheckboxState"]);
            prnt("GameName: " + FriendlyYYPName);
            prnt("RuntimePath: " + RuntimePath);
            prnt("Configuration name: " + ProjectConfigBox.Text);
            prnt("Loaded!");
        }

        private void SaveSettings()
        {
            var parser = new FileIniDataParser();
            IniData data = parser.ReadFile(AppDir + "RussellNX.ini");
            data["Main"]["AppYYPPath"] = ProjectPathBox.Text;
            data["Main"]["AppID"] = TitleIDBox.Text;
            data["Main"]["AppName"] = GameNameBox.Text;
            data["Main"]["AppAuthor"] = AuthorBox.Text;
            data["Main"]["AppVersion"] = VersionBox.Text;
            data["Main"]["AppKeysPath"] = KeysBox.Text;
            data["Main"]["AppIconPath"] = GameIconPath;
            data["Main"]["RuntimeVersion"] = RuntimeVerString;
            data["Main"]["CheckboxState"] = RetCheckboxStr();
            data["Main"]["ConfigurationName"] = ProjectConfigBox.Text;
            data["Main"]["IncludeYYdebug"] = IncludeYYdebugCheckbox.Checked ? "Yes" : "No";
            data["AppVersion"]["RNXVer"] = RNXVersionString;
            parser.WriteFile(AppDir + "RussellNX.ini", data);
            prnt("Saved!");
        }

        private void DefaultSettings()
        {
            //Migrate KeysFilePath 1.0 config
            if (File.Exists(AppDir + "KeysFilePath"))
            {
                string kpath = File.ReadAllText(AppDir + "KeysFilePath");
                KeysBox.Text = kpath;
                File.Delete(AppDir + "KeysFilePath");
            }

            //Populate default settings file.
            string fname = AppDir + "RussellNX.ini";
            File.WriteAllText(fname, "");
            var parser = new FileIniDataParser();
            IniData data = parser.ReadFile(fname);
            data.Sections.AddSection("Main");
            data.Sections.GetSectionData("Main").Comments.Add(" This is a RussellNX configuration file, edit with caution.");
            parser.WriteFile(fname, data);
            prnt("Default RussellNX.ini was made.");
            Focus(); //Focus MainForm
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            //Save settings
            prnt("Saving...");
            SaveSettings();

        }

        public void CopyLang(CheckBox c, string dir)
        {
            string langName = c.Text;
            if (c.Checked)
            {
                prnt("Copying " + GameIconPath + " to " + dir + " ...");
                File.Copy(GameIconPath, dir + "icon_" + langName + ".dat");
            }
        }

        public string RetCheckboxStr()
        {
            string ret = "";
            ret += aengCheckbox.Checked ? "1" : "0";
            ret += freCheckbox.Checked ? "1" : "0";
            ret += spaCheckbox.Checked ? "1" : "0";
            ret += itaCheckbox.Checked ? "1" : "0";
            ret += rusCheckbox.Checked ? "1" : "0";
            ret += dutCheckbox.Checked ? "1" : "0";
            ret += porCheckbox.Checked ? "1" : "0";
            ret += gerCheckbox.Checked ? "1" : "0";

            ret += DataLossCheckbox.Checked ? "1" : "0";
            ret += StartupAccCheckbox.Checked ? "1" : "0";

            return ret;
        }

        public void LoadCheckboxStr(string str)
        {
            if (str == null) return;
            //aengCheckbox.Checked = str.Substring(0, 1) == "1" ? true : false;
            freCheckbox.Checked = str.Substring(1, 1) == "1";
            spaCheckbox.Checked = str.Substring(2, 1) == "1";
            itaCheckbox.Checked = str.Substring(3, 1) == "1";
            rusCheckbox.Checked = str.Substring(4, 1) == "1";
            dutCheckbox.Checked = str.Substring(5, 1) == "1";
            porCheckbox.Checked = str.Substring(6, 1) == "1";
            gerCheckbox.Checked = str.Substring(7, 1) == "1";

            DataLossCheckbox.Checked = str.Substring(8, 1) == "1";
            StartupAccCheckbox.Checked = str.Substring(9, 1) == "1";

            return;
        }

        private void ProjectSettingsBtn_Click(object sender, EventArgs e)
        {
            if (!File.Exists(ProjectPathBox.Text)) return;
            if (!CheckSwitchOptions()) return;
            var frm = new ProjectSettings(ProjectPathBox.Text, RuntimePath);
            frm.ShowDialog();
        }

        private bool CheckSwitchOptions()
        {
            var fpath = Path.GetDirectoryName(ProjectPathBox.Text) + Path.DirectorySeparatorChar + "options" + Path.DirectorySeparatorChar + "switch" + Path.DirectorySeparatorChar;
            if (!File.Exists(fpath + "options_switch.yy"))
            {
                var ci = CultureInfo.CurrentUICulture.Name;
                string nxName = "Your project doesn't seem to have Switch options generated.\nWould you like to generate them?\n(WARNING: DON'T FORGET TO BACKUP YOUR PROJECT BEFORE CLICKING YES!)";
                if (ci == "ru-RU") nxName = "Ой ой. В вашем проекте не сгенерирован файл настроек свитча. (это нормально если вы не используйте крякнутый гмс2)\nХотите я попробую его сгенерировать? (советую вам сделать бэкап а то хер его знает)";

                var ret = MessageBox.Show(nxName, "Uh, hm...", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (ret == DialogResult.Yes)
                {
                    Directory.CreateDirectory(fpath);

                    string nnName = "Is your project a GMS2.3+ one?";
                    if (ci == "ru-RU") nnName = "У вас GMS2.3+ проект?";

                    var nret = MessageBox.Show(nnName, "?", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                    if (nret == DialogResult.No) PatchYYPMainline(fpath);
                    else PatchYYPNuBeta(fpath);
                }
                else return false;
            }

            return true;
        }

        private void PatchYYPNuBeta(string fpath)
        {
            // Write .yy file from Embedded Resources.
            var fpath2 = fpath + "options_switch.yy";
            var r = new ResourceManager(this.GetType());
            byte[] b = (byte[])r.GetObject("options_switch_nubeta"); // embedded resources don't have extensions.
            File.WriteAllBytes(fpath2, b);

            // Modify root .yyp
            List<string> bigProj = ReadAllList(ProjectPathBox.Text);
            int windowsLineInd = bigProj.IndexOf(bigProj.Where(l => l.Contains("    {\"name\":\"Windows\",\"path\":\"")).First());
            bigProj.Insert(++windowsLineInd, @"    {""name"":""Switch"",""path"":""options/switch/options_switch.yy"",},");

            File.Copy(ProjectPathBox.Text, ProjectPathBox.Text + ".rnxbk");
            File.WriteAllLines(ProjectPathBox.Text, ListToArray(bigProj), Encoding.UTF8);
            prnt("Backed up NuBeta .yyp as " + ProjectPathBox.Text + ".rnxbk (if something had gone wrong, delete broken .yyp and remove .rnxbk extension.)");
            prnt("NuBeta Switch settings generated!");
        }

        private void PatchYYPMainline(string fpath)
        {
            // Write .yy file from Embedded Resources.
            var fpath2 = fpath + "options_switch.yy";
            var r = new ResourceManager(this.GetType());
            byte[] b = (byte[])r.GetObject("options_switch"); // embedded resources don't have extensions.
            File.WriteAllBytes(fpath2, b);

            // Modify root .yyp
            List<string> bigProj = ReadAllList(ProjectPathBox.Text);
            var resGuid = Guid.NewGuid();
            int windowsLineInd = bigProj.IndexOf(bigProj.Where(l => l.Contains("\"resourceType\": \"GMWindowsOptions\"")).First());
            windowsLineInd += 2; // skip "}" and "},"

            // construct GMSwitchOptions entry.
            bigProj.Insert(++windowsLineInd, "        {");
            bigProj.Insert(++windowsLineInd, "            \"Key\": \"3a5af38c-757d-41ae-98c0-5d4b09e14e6a\",");
            bigProj.Insert(++windowsLineInd, "            \"Value\": {");
            bigProj.Insert(++windowsLineInd,$"                \"id\": \"{resGuid}\",");
            bigProj.Insert(++windowsLineInd,@"                ""resourcePath"": ""options\\switch\\options_switch.yy"",");
            bigProj.Insert(++windowsLineInd, "                \"resourceType\": \"GMSwitchOptions\"");
            bigProj.Insert(++windowsLineInd, "            }");
            bigProj.Insert(++windowsLineInd, "        },");

            File.Copy(ProjectPathBox.Text, ProjectPathBox.Text + ".nubk");
            File.WriteAllLines(ProjectPathBox.Text, ListToArray(bigProj), Encoding.UTF8);
            prnt("Backed up .yyp as " + ProjectPathBox.Text + ".nubk (if something had gone wrong, delete broken .yyp and remove .nubk extension.)");
            prnt("Switch settings generated!");
        }

        private List<string> ReadAllList(string filePath)
        {
            var array = File.ReadAllLines(filePath, Encoding.UTF8);
            var list = new List<string>(array.Length);
            for (int i = 0; i < array.Length; i++)
                list.Add(array[i]);

            return list;
        }

        private string[] ListToArray(List<string> list)
        {
            string[] ret = new string[list.Count];
            for (int i = 0; i < ret.Length; i++)
                ret[i] = list[i];

            return ret;
        }

        private void ExportLogBtn_Click(object sender, EventArgs e)
        {
            var dialog = new SaveFileDialog();
            dialog.DefaultExt = ".log";
            dialog.InitialDirectory = AppDir;
            dialog.AddExtension = true;
            dialog.Title = "Choose where to save the log.";
            dialog.Filter = "Log files (*.log)|*.log";
            Stream myStream;
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                if ((myStream = dialog.OpenFile()) != null)
                {
                    byte[] to_write = Encoding.UTF8.GetBytes(LogBox.Text);
                    myStream.Write(to_write, 0, to_write.Length);
                    myStream.Close();
                    prnt("Log has been saved.");
                }
            }
            dialog.Dispose();
        }

        private void CleanLogBtn_Click(object sender, EventArgs e)
        {
            prnt("$LOG_CLEAN");
        }

        private bool IsMainlineActuallyNuBeta(string ver)
        {
            string[] numbers = ver.Split('.');
            int first = int.Parse(numbers[0]);
            int second = int.Parse(numbers[1]);
            // 2.3.232323
            // [1] == 3
            // 3 >= 3 -> nubeta
            // otherwise, mainline.
            return first > 2000 || second >= 3;
        }

        private void SearchForRuntimes(string ci)
        {
            string MainlinePath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\GameMakerStudio2\\Cache\\runtimes\\";
            if (Directory.Exists(MainlinePath))
            {
                // look out for 2.0-2.2.5 runtimes
                foreach (var dinfo in Directory.EnumerateDirectories(MainlinePath))
                {
                    string runtimever = Path.GetFileName(dinfo).Replace("runtime-", string.Empty); // looks weird, I know.
                    var runtime = new RuntimeVersion() { Version = runtimever, IsNuBeta = IsMainlineActuallyNuBeta(runtimever), FullPath = dinfo };

                    RuntimeChooserBox.Items.Add(runtime);
                }
            }
            else
            {
                string enMsg = "Excuse me, you don't have GMS 2? Who the cat are you?";
                string ruMsg = "Прощу прощения мистар геймдевелопер, но у вас нет гмс2? енто как? :)";
                MessageBox.Show(ci == "ru-RU" ? ruMsg : enMsg, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(-1);
            }

            string NuBetaPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\GameMakerStudio2-Beta\\Cache\\runtimes\\";
            if (Directory.Exists(NuBetaPath))
            {
                // look out for "NuBeta" runtimes (>=2.3)
                foreach (var dinfo in Directory.EnumerateDirectories(NuBetaPath))
                {
                    string runtimever = Path.GetFileName(dinfo).Replace("runtime-", string.Empty); // looks weird, I know.
                    var runtime = new RuntimeVersion() { Version = runtimever, IsNuBeta = true, FullPath = dinfo };
                    RuntimeChooserBox.Items.Add(runtime);
                }
            }
        }

        private void MainForm_Load(object sender, EventArgs _e)
        {
            ApplyLocalisation();

            var ci = CultureInfo.CurrentUICulture.Name;
            string errStr = "ERROR!\nYour current RussellNX directory doesn't have Read Write permissions.\nPlease move RussellNX to Desktop, Documents, Downloads heck, anywhere else!\n\nException: ";
            if (ci == "ru-RU") errStr = "ОШИБКА!\nRussellNX не имеет прав на чтение/запись в папке в которую вы его распаковали, переместите RussellNX в другое место. Детали: ";

            //Check for write access first.
            try { File.WriteAllText(AppDir + "dircheck.txt", ""); File.Delete(AppDir + "dircheck.txt"); }
            catch (Exception e)
            {
                //for some reason this messagebox doesn't wanna show up (??)
                MessageBox.Show(errStr + e.ToString(), "Idiot Check Fail", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(-1);
            }
            //I mean, if this check passed, this file should exist, if check failed, program exits before this line executes.

            // autoupdater is deprecated.

            prnt("WARNING: Installing Custom NSPs may get your Switch banned, be careful!\n");

            SearchForRuntimes(ci);

            if (!File.Exists(AppDir + "RussellNX.ini")) DefaultSettings(); else LoadSettings();

            //Set version label
            RNXVersionLabel.Text = RNXVersionLabel.Text.Replace("0.0.0", RNXVersionString);

            if (!File.Exists(GameIconPath))
            {
                if (!File.Exists(AppDir + "default_icon.png"))
                {
                    Properties.Resources.default_icon.Save(AppDir + "default_icon.png");
                }
                GameIconPath = AppDir + "default_icon.png";
            }

            IconPicBox.Image = new Bitmap(GameIconPath);

            //Check for 2.2.3.344 Runtime
            //other runtimes maybe later idk...
            string runStr = "ERROR!\nCannot find runtime. This error is not fatal and can happen if you only have the latest runtime.\nYou can download RussellNX's recommended runtime GMS 2 in File->Preferences->Runtime Feeds->Master (the tool is using 2.2.3.344 as default)";
            if (ci == "ru-RU") runStr = "ОШИБКА!\nНе могу найти рантайм. Ошибка не фатальная и просто значит что рантайм не установлен.\nВы сможете поменять версию рантайма в программе или можете скачать рекоммендуемый рантайм 2.2.3.344 в GMS 2.";
            if (!File.Exists(RuntimePath + "\\bin\\GMAssetCompiler.exe") && !File.Exists(RuntimePath + "\\bin\\assetcompiler\\windows\\x64\\GMAssetCompiler.exe"))
            {
                MessageBox.Show(runStr, "ERROR!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            CleanupTempDirs();
            CheckForKeys(ci);
            RandomQuotePrint(ci);
            prnt("RussellNX Version " + RNXVersionString + " is waiting for you, master!");
            prnt("I wish I could visit beautiful cities like Toronto or London, but I guess not today... :<");
            prnt("Do people still use this thing?");
        }

        private void RuntimeChooserBox_SelectedValueChanged(object sender, EventArgs e)
        {
            RuntimeVersion item = (RuntimeVersion)RuntimeChooserBox.SelectedItem;
            RuntimeVerString = item.Version;
            RuntimePath = item.FullPath;

            //prnt("RuntimeVer: " + RuntimeVerString);
        }
    }
}

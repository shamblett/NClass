﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace NClass.CodeGenerator {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "15.9.0.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("4")]
        public int IndentSize {
            get {
                return ((int)(this["IndentSize"]));
            }
            set {
                this["IndentSize"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool UseTabsForIndents {
            get {
                return ((bool)(this["UseTabsForIndents"]));
            }
            set {
                this["UseTabsForIndents"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute(@"<?xml version=""1.0"" encoding=""utf-16""?>
<ArrayOfString xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
  <string>System</string>
  <string>System.Collections.Generic</string>
  <string>System.Linq</string>
  <string>System.Text</string>
  <string>System.Threading.Tasks</string>
</ArrayOfString>")]
        public global::System.Collections.Specialized.StringCollection CSharpImportList {
            get {
                return ((global::System.Collections.Specialized.StringCollection)(this["CSharpImportList"]));
            }
            set {
                this["CSharpImportList"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute(@"<?xml version=""1.0"" encoding=""utf-16""?>
<ArrayOfString xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
  <string>import 'dart:io';</string></ArrayOfString>")]
        public global::System.Collections.Specialized.StringCollection DartImportList
        {
            get
            {
                return ((global::System.Collections.Specialized.StringCollection)(this["DartImportList"]));
            }
            set
            {
                this["DartImportList"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("<?xml version=\"1.0\" encoding=\"utf-16\"?>\r\n<ArrayOfString xmlns:xsi=\"http://www.w3." +
            "org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">\r\n  <s" +
            "tring>java.io.*</string>\r\n  <string>java.util.*</string>\r\n</ArrayOfString>")]
        public global::System.Collections.Specialized.StringCollection JavaImportList {
            get {
                return ((global::System.Collections.Specialized.StringCollection)(this["JavaImportList"]));
            }
            set {
                this["JavaImportList"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("VisualStudio2017")]
        public global::NClass.CodeGenerator.SolutionType SolutionType {
            get {
                return ((global::NClass.CodeGenerator.SolutionType)(this["SolutionType"]));
            }
            set {
                this["SolutionType"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string DestinationPath {
            get {
                return ((string)(this["DestinationPath"]));
            }
            set {
                this["DestinationPath"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool UseNotImplementedExceptions {
            get {
                return ((bool)(this["UseNotImplementedExceptions"]));
            }
            set {
                this["UseNotImplementedExceptions"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool UseRootNamespace {
            get {
                return ((bool)(this["UseRootNamespace"]));
            }
            set {
                this["UseRootNamespace"] = value;
            }
        }
    }
}

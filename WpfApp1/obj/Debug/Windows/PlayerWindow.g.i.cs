﻿#pragma checksum "..\..\..\Windows\PlayerWindow.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "ACA0C9915D773F124D990B71688E6F9E"
//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан программой.
//     Исполняемая версия:4.0.30319.42000
//
//     Изменения в этом файле могут привести к неправильной работе и будут потеряны в случае
//     повторной генерации кода.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;
using WpfApp1;


namespace WpfApp1.Windows {
    
    
    internal partial class PlayerWindow : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 48 "..\..\..\Windows\PlayerWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Label dateDay;
        
        #line default
        #line hidden
        
        
        #line 49 "..\..\..\Windows\PlayerWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Label dateMonth;
        
        #line default
        #line hidden
        
        
        #line 50 "..\..\..\Windows\PlayerWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Label dateYear;
        
        #line default
        #line hidden
        
        
        #line 53 "..\..\..\Windows\PlayerWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Label timeHour;
        
        #line default
        #line hidden
        
        
        #line 54 "..\..\..\Windows\PlayerWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Label timeMin;
        
        #line default
        #line hidden
        
        
        #line 55 "..\..\..\Windows\PlayerWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Label timeSec;
        
        #line default
        #line hidden
        
        
        #line 61 "..\..\..\Windows\PlayerWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.CheckBox readStock;
        
        #line default
        #line hidden
        
        
        #line 62 "..\..\..\Windows\PlayerWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.CheckBox readTicks;
        
        #line default
        #line hidden
        
        
        #line 63 "..\..\..\Windows\PlayerWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.CheckBox readOwns;
        
        #line default
        #line hidden
        
        
        #line 64 "..\..\..\Windows\PlayerWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.CheckBox readMsgs;
        
        #line default
        #line hidden
        
        
        #line 69 "..\..\..\Windows\PlayerWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ListView fileList;
        
        #line default
        #line hidden
        
        
        #line 110 "..\..\..\Windows\PlayerWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button buttonStart;
        
        #line default
        #line hidden
        
        
        #line 114 "..\..\..\Windows\PlayerWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Primitives.ToggleButton buttonPause;
        
        #line default
        #line hidden
        
        
        #line 118 "..\..\..\Windows\PlayerWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button buttonStop;
        
        #line default
        #line hidden
        
        
        #line 120 "..\..\..\Windows\PlayerWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock dateTimePointer;
        
        #line default
        #line hidden
        
        
        #line 124 "..\..\..\Windows\PlayerWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button buttonAdd;
        
        #line default
        #line hidden
        
        
        #line 128 "..\..\..\Windows\PlayerWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button buttonRmv;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/WpfApp1;component/windows/playerwindow.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\Windows\PlayerWindow.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            this.dateDay = ((System.Windows.Controls.Label)(target));
            return;
            case 2:
            this.dateMonth = ((System.Windows.Controls.Label)(target));
            return;
            case 3:
            this.dateYear = ((System.Windows.Controls.Label)(target));
            return;
            case 4:
            this.timeHour = ((System.Windows.Controls.Label)(target));
            return;
            case 5:
            this.timeMin = ((System.Windows.Controls.Label)(target));
            return;
            case 6:
            this.timeSec = ((System.Windows.Controls.Label)(target));
            return;
            case 7:
            this.readStock = ((System.Windows.Controls.CheckBox)(target));
            
            #line 61 "..\..\..\Windows\PlayerWindow.xaml"
            this.readStock.Unchecked += new System.Windows.RoutedEventHandler(this.StreamFlagChanged);
            
            #line default
            #line hidden
            return;
            case 8:
            this.readTicks = ((System.Windows.Controls.CheckBox)(target));
            
            #line 62 "..\..\..\Windows\PlayerWindow.xaml"
            this.readTicks.Checked += new System.Windows.RoutedEventHandler(this.StreamFlagChanged);
            
            #line default
            #line hidden
            
            #line 62 "..\..\..\Windows\PlayerWindow.xaml"
            this.readTicks.Unchecked += new System.Windows.RoutedEventHandler(this.StreamFlagChanged);
            
            #line default
            #line hidden
            return;
            case 9:
            this.readOwns = ((System.Windows.Controls.CheckBox)(target));
            
            #line 63 "..\..\..\Windows\PlayerWindow.xaml"
            this.readOwns.Checked += new System.Windows.RoutedEventHandler(this.ReadOwnsChanged);
            
            #line default
            #line hidden
            
            #line 63 "..\..\..\Windows\PlayerWindow.xaml"
            this.readOwns.Unchecked += new System.Windows.RoutedEventHandler(this.ReadOwnsChanged);
            
            #line default
            #line hidden
            return;
            case 10:
            this.readMsgs = ((System.Windows.Controls.CheckBox)(target));
            
            #line 64 "..\..\..\Windows\PlayerWindow.xaml"
            this.readMsgs.Checked += new System.Windows.RoutedEventHandler(this.StreamFlagChanged);
            
            #line default
            #line hidden
            
            #line 64 "..\..\..\Windows\PlayerWindow.xaml"
            this.readMsgs.Unchecked += new System.Windows.RoutedEventHandler(this.StreamFlagChanged);
            
            #line default
            #line hidden
            return;
            case 11:
            this.fileList = ((System.Windows.Controls.ListView)(target));
            
            #line 69 "..\..\..\Windows\PlayerWindow.xaml"
            this.fileList.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(this.fileList_SelectionChanged);
            
            #line default
            #line hidden
            return;
            case 12:
            this.buttonStart = ((System.Windows.Controls.Button)(target));
            
            #line 110 "..\..\..\Windows\PlayerWindow.xaml"
            this.buttonStart.Click += new System.Windows.RoutedEventHandler(this.StartPlay);
            
            #line default
            #line hidden
            return;
            case 13:
            this.buttonPause = ((System.Windows.Controls.Primitives.ToggleButton)(target));
            
            #line 114 "..\..\..\Windows\PlayerWindow.xaml"
            this.buttonPause.Click += new System.Windows.RoutedEventHandler(this.buttonPause_Click);
            
            #line default
            #line hidden
            return;
            case 14:
            this.buttonStop = ((System.Windows.Controls.Button)(target));
            
            #line 118 "..\..\..\Windows\PlayerWindow.xaml"
            this.buttonStop.Click += new System.Windows.RoutedEventHandler(this.StopPlay);
            
            #line default
            #line hidden
            return;
            case 15:
            this.dateTimePointer = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 16:
            this.buttonAdd = ((System.Windows.Controls.Button)(target));
            
            #line 124 "..\..\..\Windows\PlayerWindow.xaml"
            this.buttonAdd.Click += new System.Windows.RoutedEventHandler(this.buttonAdd_Click);
            
            #line default
            #line hidden
            return;
            case 17:
            this.buttonRmv = ((System.Windows.Controls.Button)(target));
            
            #line 128 "..\..\..\Windows\PlayerWindow.xaml"
            this.buttonRmv.Click += new System.Windows.RoutedEventHandler(this.buttonRmv_Click);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}


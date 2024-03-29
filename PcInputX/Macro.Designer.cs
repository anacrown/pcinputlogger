// ------------------------------------------------------------------------------
//  <auto-generated>
//    Generated by Xsd2Code++. Version 4.2.0.72
//    <NameSpace>PcInputLogger</NameSpace><Collection>List</Collection><codeType>CSharp</codeType><EnableDataBinding>False</EnableDataBinding><GenerateCloneMethod>False</GenerateCloneMethod><GenerateDataContracts>False</GenerateDataContracts><DataMemberNameArg>OnlyIfDifferent</DataMemberNameArg><DataMemberOnXmlIgnore>False</DataMemberOnXmlIgnore><CodeBaseTag>Net20</CodeBaseTag><InitializeFields>All</InitializeFields><GenerateUnusedComplexTypes>False</GenerateUnusedComplexTypes><GenerateUnusedSimpleTypes>False</GenerateUnusedSimpleTypes><GenerateXMLAttributes>True</GenerateXMLAttributes><OrderXMLAttrib>False</OrderXMLAttrib><EnableLazyLoading>False</EnableLazyLoading><VirtualProp>False</VirtualProp><PascalCase>False</PascalCase><AutomaticProperties>False</AutomaticProperties><PropNameSpecified>None</PropNameSpecified><PrivateFieldName>StartWithUnderscore</PrivateFieldName><PrivateFieldNamePrefix></PrivateFieldNamePrefix><EnableRestriction>False</EnableRestriction><RestrictionMaxLenght>False</RestrictionMaxLenght><RestrictionRegEx>False</RestrictionRegEx><RestrictionRange>False</RestrictionRange><ValidateProperty>False</ValidateProperty><ClassNamePrefix></ClassNamePrefix><ClassLevel>Public</ClassLevel><PartialClass>True</PartialClass><ClassesInSeparateFiles>False</ClassesInSeparateFiles><ClassesInSeparateFilesDir></ClassesInSeparateFilesDir><TrackingChangesEnable>False</TrackingChangesEnable><GenTrackingClasses>False</GenTrackingClasses><HidePrivateFieldInIDE>False</HidePrivateFieldInIDE><EnableSummaryComment>False</EnableSummaryComment><EnableAppInfoSettings>False</EnableAppInfoSettings><EnableExternalSchemasCache>False</EnableExternalSchemasCache><EnableDebug>False</EnableDebug><EnableWarn>False</EnableWarn><ExcludeImportedTypes>False</ExcludeImportedTypes><ExpandNesteadAttributeGroup>False</ExpandNesteadAttributeGroup><CleanupCode>False</CleanupCode><EnableXmlSerialization>False</EnableXmlSerialization><SerializeMethodName>Serialize</SerializeMethodName><DeserializeMethodName>Deserialize</DeserializeMethodName><SaveToFileMethodName>SaveToFile</SaveToFileMethodName><LoadFromFileMethodName>LoadFromFile</LoadFromFileMethodName><EnableEncoding>False</EnableEncoding><EnableXMLIndent>False</EnableXMLIndent><IndentChar>Indent2Space</IndentChar><NewLineAttr>False</NewLineAttr><OmitXML>False</OmitXML><Encoder>UTF8</Encoder><Serializer>XmlSerializer</Serializer><sspNullable>False</sspNullable><sspString>False</sspString><sspCollection>False</sspCollection><sspComplexType>False</sspComplexType><sspSimpleType>False</sspSimpleType><sspEnumType>False</sspEnumType><XmlSerializerEvent>False</XmlSerializerEvent><BaseClassName>EntityBase</BaseClassName><UseBaseClass>False</UseBaseClass><GenBaseClass>False</GenBaseClass><CustomUsings></CustomUsings><AttributesToExlude></AttributesToExlude>
//  </auto-generated>
// ------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;

#pragma warning disable
namespace PcInputX
{
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.7.2102.0")]
    [Serializable()]
    [DebuggerStepThrough()]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true, Namespace = "http://tempuri.org/Macro.xsd")]
    [XmlRoot(Namespace = "http://tempuri.org/Macro.xsd", IsNullable = false)]
    public partial class TMacro
    {

        #region Private fields
        private List<TEvent> _event;
        #endregion

        public TMacro()
        {
            this._event = new List<TEvent>();
        }

        [XmlElement("Event")]
        public List<TEvent> Event
        {
            get
            {
                return this._event;
            }
            set
            {
                this._event = value;
            }
        }
    }

    [XmlInclude(typeof(TKeyboard))]
    [XmlInclude(typeof(TMouse))]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.7.2102.0")]
    [Serializable()]
    [DebuggerStepThrough()]
    [DesignerCategory("code")]
    [XmlType(Namespace = "http://tempuri.org/Macro.xsd")]
    public partial class TEvent
    {

        #region Private fields
        private TXEventType _eventType;

        private int _duration;
        #endregion

        [XmlAttribute()]
        public TXEventType EventType
        {
            get
            {
                return this._eventType;
            }
            set
            {
                this._eventType = value;
            }
        }

        [XmlAttribute()]
        public int Duration
        {
            get
            {
                return this._duration;
            }
            set
            {
                this._duration = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.7.2102.0")]
    [Serializable()]
    [XmlType(Namespace = "http://tempuri.org/Macro.xsd")]
    public enum TXEventType
    {

        /// <remarks/>
        WM_LBUTTONDOWN,

        /// <remarks/>
        WM_LBUTTONUP,

        /// <remarks/>
        WM_MOUSEMOVE,

        /// <remarks/>
        WM_MOUSEWHEEL,

        /// <remarks/>
        WM_RBUTTONDOWN,

        /// <remarks/>
        WM_RBUTTONUP,

        /// <remarks/>
        WM_KEYDOWN,

        /// <remarks/>
        WM_KEYUP,

        /// <remarks/>
        WM_SYSKEYDOWN,

        /// <remarks/>
        WM_SYSKEYUP,
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.7.2102.0")]
    [Serializable()]
    [DebuggerStepThrough()]
    [DesignerCategory("code")]
    [XmlType(Namespace = "http://tempuri.org/Macro.xsd")]
    public partial class TKeyboard : TEvent
    {

        #region Private fields
        private string _keyName;

        private uint _keyCode;
        #endregion

        [XmlAttribute()]
        public string KeyName
        {
            get
            {
                return this._keyName;
            }
            set
            {
                this._keyName = value;
            }
        }

        [XmlAttribute()]
        public uint KeyCode
        {
            get
            {
                return this._keyCode;
            }
            set
            {
                this._keyCode = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.7.2102.0")]
    [Serializable()]
    [DebuggerStepThrough()]
    [DesignerCategory("code")]
    [XmlType(Namespace = "http://tempuri.org/Macro.xsd")]
    public partial class TMouse : TEvent
    {

        #region Private fields
        private int _delta;

        private int _x;

        private int _y;
        #endregion

        [XmlAttribute()]
        public int Delta
        {
            get
            {
                return this._delta;
            }
            set
            {
                this._delta = value;
            }
        }

        [XmlAttribute()]
        public int X
        {
            get
            {
                return this._x;
            }
            set
            {
                this._x = value;
            }
        }

        [XmlAttribute()]
        public int Y
        {
            get
            {
                return this._y;
            }
            set
            {
                this._y = value;
            }
        }
    }
}
#pragma warning restore

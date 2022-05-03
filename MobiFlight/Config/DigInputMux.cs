﻿using System;
using System.Linq;
using System.Xml.Serialization;

namespace MobiFlight.Config
{
    public class DigInputMux : BaseDevice
    {
        //The selector is serialized without specifying [XmlAttribute], because it is serializable itself
        //TODO: should we not prevent including Selector in the clients'serialization?
        // This might be seen as consistent with including the selector pins in the config messages
        // (which is in itself arguable at least), but it has no real use and it might might lead to "strange" issues
        // if the config file is tampered with.
        public MobiFlight.Config.MuxDriver Selector;

        const ushort _paramCount = 7;
        [XmlAttribute]
        public String DataPin = "-1";
        [XmlAttribute]
        public String NumModules = "2"; // defaults to CD4067

        public DigInputMux()
        {
            Name = "DigInputMux";
            _type = DeviceType.DigInputMux;
            _muxClient = true;
            Selector = null;
        }

        public DigInputMux(MobiFlight.Config.MuxDriver muxSelector) { 
            Name = "DigInputMux"; 
            _type = DeviceType.DigInputMux;
            _muxClient = true;
            Selector = muxSelector;
        }

        public void setDriver(MobiFlight.Config.MuxDriver muxSelector)
        {
            Selector = muxSelector;
        }



        override public String ToInternal()
        {
            string dummySel = "-1" + Separator + "-1" + Separator + "-1" + Separator + "-1" + Separator;
            return base.ToInternal() + Separator
                 + DataPin + Separator
                 // Selector pins, always sent
                 + (Selector?.ToInternalStripped() ?? dummySel)
                 + NumModules + Separator
                 + Name + End;
        }

        override public bool FromInternal(String value)
        {
            if (value.Length == value.IndexOf(End) + 1) value = value.Substring(0, value.Length - 1);
            String[] paramList = value.Split(Separator);
            if (paramList.Count() != _paramCount + 1)
            {
                throw new ArgumentException("Param count does not match. " + paramList.Count() + " given, " + _paramCount + " expected");
            }

            DataPin     = paramList[1];
            NumModules  = paramList[6];
            Name        = paramList[7];

            // pass the MuxDriver pins, but only if the muxDriver wasn't already set
            if (Selector == null || Selector.isInitialized()) return false;
            value = ((int)DeviceType.MuxDriver).ToString() + Separator + paramList[2] + Separator + paramList[3] + Separator + paramList[4] + Separator + paramList[5] + End;
            Selector.FromInternal(value);
            // The FromInternal() call takes care internally of the activation counter and the "initialized" flag
            return true;
        }

        public override string ToString()
        {
            return $"{Type}:{Name}";
        }
    }
}
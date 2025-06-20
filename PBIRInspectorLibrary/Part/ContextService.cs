﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PBIRInspectorLibrary.Part
{
    //TODO: not thread safe
    internal class ContextService
    {
        private static ContextService instance = null;

        internal IPartQuery PartQuery { get; set; }

        internal Part Part { get; set; }

        private ContextService() { }

        public static ContextService GetInstance()
        {
            if (instance == null)
            {
                instance = new ContextService();
            }
            return instance;
        }

    }
}

﻿// NClass - Free class diagram editor
// Copyright (C) 2006-2009 Balazs Tihanyi
// Copyright (C) 2020 Georgi Baychev

// This program is free software; you can redistribute it and/or modify it under 
// the terms of the GNU General Public License as published by the Free Software 
// Foundation; either version 3 of the License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful, but WITHOUT 
// ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS 
// FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License along with 
// this program; if not, write to the Free Software Foundation, Inc., 
// 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA

using System;
using System.Text;
using System.Collections.Generic;
using NClass.Core;
using NClass.Translations;

namespace NClass.Dart
{
    internal sealed class DartClass : ClassType
    {
        internal DartClass() : this("NewClass")
        {
        }

        /// <exception cref="BadSyntaxException">
        /// The <paramref name="name"/> does not fit to the syntax.
        /// </exception>
        internal DartClass(string name) : base(name)
        {
        }

        /// <exception cref="BadSyntaxException">
        /// The type visibility is not valid in the current context.
        /// </exception>
        public override AccessModifier AccessModifier
        {
            get
            {
                return base.AccessModifier;
            }
            set
            {
                if (IsTypeNested ||
                    value == AccessModifier.Default ||
                    value == AccessModifier.Internal ||
                    value == AccessModifier.Public)
                {
                    base.AccessModifier = value;
                }
            }
        }

        /// <exception cref="RelationshipException">
        /// The language of <paramref name="value"/> does not equal.-or-
        /// <paramref name="value"/> is static or sealed class.-or-
        /// The <paramref name="value"/> is descendant of the class.
        /// </exception>
        public override ClassType BaseClass
        {
            get
            {
                if (base.BaseClass == null && this != DartLanguage.ObjectClass)
                    return DartLanguage.ObjectClass;
                else
                    return base.BaseClass;
            }
            set
            {
                base.BaseClass = value;
            }
        }

        public override AccessModifier DefaultAccess
        {
            get { return AccessModifier.Internal; }
        }

        public override AccessModifier DefaultMemberAccess
        {
            get { return AccessModifier.Private; }
        }

        public override bool SupportsProperties
        {
            get { return true; }
        }

        public override bool SupportsEvents
        {
            get { return true; }
        }

        public override bool SupportsDestructors
        {
            get { return true; }
        }

        /// <exception cref="ArgumentException">
        /// The <paramref name="value"/> is already a child member of the type.
        /// </exception>
        public override INestable NestingParent
        {
            get
            {
                return base.NestingParent;
            }

            set
            {
                try
                {
                    RaiseChangedEvent = false;

                    base.NestingParent = value;
                    if (NestingParent == null && Access != AccessModifier.Public)
                        AccessModifier = AccessModifier.Internal;
                }
                finally
                {
                    RaiseChangedEvent = true;
                }
            }
        }

        public override Language Language
        {
            get { return DartLanguage.Instance; }
        }

        /// <exception cref="RelationshipException">
        /// The language of <paramref name="interfaceType"/> does not equal.-or-
        /// <paramref name="interfaceType"/> is earlier implemented interface.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="interfaceType"/> is null.
        /// </exception>
        public override void AddInterface(InterfaceType interfaceType)
        {
            if (!(interfaceType is DartInterface))
                throw new RelationshipException(string.Format(Strings.ErrorInterfaceLanguage, "C#"));

            base.AddInterface(interfaceType);
        }

        /// <exception cref="BadSyntaxException">
        /// The <paramref name="name"/> does not fit to the syntax.
        /// </exception>
        public override Field AddField()
        {
            Field field = new DartField(this);

            AddField(field);
            return field;
        }

        public override Constructor AddConstructor()
        {
            Constructor constructor = new DartConstructor(this);

            if (Modifier == ClassModifier.Abstract)
                constructor.AccessModifier = AccessModifier.Protected;
            else if (Modifier != ClassModifier.Static)
                constructor.AccessModifier = AccessModifier.Public;

            AddOperation(constructor);
            return constructor;
        }

        public override Destructor AddDestructor() { return null; }

        /// <exception cref="BadSyntaxException">
        /// The <paramref name="name"/> does not fit to the syntax.
        /// </exception>
        public override Method AddMethod()
        {
            Method method = new DartMethod(this);

            method.AccessModifier = AccessModifier.Public;
            method.IsStatic = (Modifier == ClassModifier.Static);

            AddOperation(method);
            return method;
        }

        /// <exception cref="BadSyntaxException">
        /// The <paramref name="name"/> does not fit to the syntax.
        /// </exception>
        public override Property AddProperty()
        {
            Property property = new DartProperty(this);

            property.AccessModifier = AccessModifier.Public;
            property.IsStatic = (Modifier == ClassModifier.Static);

            AddOperation(property);
            return property;
        }

        /// <exception cref="BadSyntaxException">
        /// The <paramref name="name"/> does not fit to the syntax.
        /// </exception>
        public override Event AddEvent()
        {
            return null;
        }

        public override string GetDeclaration()
        {
            StringBuilder builder = new StringBuilder();

            if (AccessModifier != AccessModifier.Default)
            {
                builder.Append(Language.GetAccessString(AccessModifier, true));
                builder.Append(" ");
            }
            if (Modifier != ClassModifier.None)
            {
                builder.Append(Language.GetClassModifierString(Modifier, true));
                builder.Append(" ");
            }
            builder.AppendFormat("class {0}", Name);

            if (HasExplicitBase || InterfaceList.Count > 0)
            {
                builder.Append(" : ");
                if (HasExplicitBase)
                {
                    builder.Append(BaseClass.Name);
                    if (InterfaceList.Count > 0)
                        builder.Append(", ");
                }
                for (int i = 0; i < InterfaceList.Count; i++)
                {
                    builder.Append(InterfaceList[i].Name);
                    if (i < InterfaceList.Count - 1)
                        builder.Append(", ");
                }
            }

            return builder.ToString();
        }

        public override ClassType Clone()
        {
            DartClass newClass = new DartClass();
            newClass.CopyFrom(this);
            return newClass;
        }

        public override INestableChild CloneChild()
        {
            return Clone();
        }
    }
}

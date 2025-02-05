﻿using System.Collections.Generic;
using System.Text.RegularExpressions;
using NClass.Core;
using NClass.Translations;

namespace NClass.Dart
{
    public class DartPropertyDeclaration : IDartPropertyDeclaration
    {
        const string AccessorAccessPattern = @"(protected\s+internal\s+|" + 
                                             @"internal\s+protected\s+|internal\s+|protected\s+|private\s+)";

        // [<access>] [<modifiers>] <type> <name>["["<args>"]"]
        // ["{"] [[<getaccess>] get[;]] [[<setaccess>] set[;]] ["}"]
        const string PropertyPattern =
            @"^\s*" + DartLanguage.AccessPattern + DartLanguage.OperationModifiersPattern +
            @"(?<type>" + DartLanguage.GenericTypePattern2 + @")\s+" +
            @"((?<name>" + DartLanguage.GenericOperationNamePattern +
            @")|(?<name>this)\s*\[(?<args>.+)\])" +
            @"\s*{?\s*(?<get>(?<getaccess>" + AccessorAccessPattern + @")?get(\s*;|\s|$))?" +
            @"\s*(?<set>(?<setaccess>" + AccessorAccessPattern + @")?set(\s*;)?)?\s*}?" +
            DartLanguage.DeclarationEnding;

        private static readonly Regex propertyRegex = new Regex(PropertyPattern);

        readonly Match match;
        readonly HashSet<string> modifiers;

        public DartPropertyDeclaration(Match match)
        {
            if (!match.Success)
            {
                throw new BadSyntaxException(Strings.ErrorInvalidDeclaration);
            }

            this.match = match;
            this.modifiers = GetModifiers(match);
        }

        public static DartPropertyDeclaration Create(string declaration)
        {
            var match = propertyRegex.Match(declaration);
            return new DartPropertyDeclaration(match);
        }

        public string Name
        {
            get { return match.Groups["name"].Value; }
        }

        public string Type
        {
            get { return match.Groups["type"].Value; }
        }

        public AccessModifier AccessModifier
        {
            get { return ParseAccessModifier(match.Groups["access"].Value); }
        }

        public IArgumentListDeclaration<IParameterDeclaration> ArgumentList
        {
            get { return DartArgumentListDeclaration.Create(match.Groups["args"].Value); }
        }

        public bool IsStatic
        {
            get { return modifiers.Contains("static"); }
        }

        public bool IsVirtual
        {
            get { return modifiers.Contains("virtual"); }
        }

        public bool IsAbstract
        {
            get { return modifiers.Contains("abstract"); }
        }

        public bool IsOverride
        {
            get { return modifiers.Contains("override"); }
        }

        public bool IsSealed
        {
            get { return modifiers.Contains("sealed"); }
        }

        public bool IsHider
        {
            get { return modifiers.Contains("new"); }
        }

        public bool IsReadonly
        {
            get { return match.Groups["get"].Success && !match.Groups["set"].Success; }
        }

        public bool IsWriteonly
        {
            get { return !match.Groups["get"].Success && match.Groups["set"].Success; }
        }

        public bool IsExplicitImplementation
        {
            get { return match.Groups["namedot"].Success; }
        }

        public AccessModifier ReadAccess
        {
            get { return ParseAccessModifier(match.Groups["getaccess"].Value); }
        }

        public AccessModifier WriteAccess
        {
            get { return ParseAccessModifier(match.Groups["setaccess"].Value); }
        }

        private static AccessModifier ParseAccessModifier(string value)
        {
            return DartLanguage.Instance.TryParseAccessModifier(value);
        }

        private static HashSet<string> GetModifiers(Match match)
        {
            var captures = match.Groups["modifier"].Captures;
            var modifiers = new HashSet<string>();

            foreach (Capture capture in captures)
            {
                modifiers.Add(capture.Value);
            }

            return modifiers;
        }
    }
}

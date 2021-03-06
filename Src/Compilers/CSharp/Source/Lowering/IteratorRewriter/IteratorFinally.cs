﻿// Copyright (c) Microsoft Open Technologies, Inc.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp
{
    partial class IteratorMethodToClassRewriter
    {
        /// <summary>
        /// A synthesized Finally method containing finalization code for a resumable try statement.
        /// Finalization code for such try may run when:
        /// 1) control flow goes out of try scope by dropping through
        /// 2) control flow goes out of try scope by conditionally or unconditionally branching outside of one ore more try/finally frames.
        /// 3) enumerator is disposed by the owner.
        /// 4) enumerator is being disposed after an exception.
        /// 
        /// It is easier to manage partial or complete finalization when every finally is factored out as a separate method. 
        /// 
        /// NOTE: Finally is a private void nonvirtual instance method with no parameters. 
        ///       It is a valid JIT inlining target as long as JIT may consider inlining profitable.
        /// </summary>
        private class IteratorFinally : SynthesizedInstanceMethodSymbol
        {
            private readonly TypeSymbol containingType;
            private readonly string name;

            public IteratorFinally(TypeSymbol containingType, string name)
            {
                this.containingType = containingType;
                this.name = name;
            }

            public override string Name
            {
                get
                {
                    return name;
                }
            }

            internal override bool IsMetadataNewSlot(bool ignoreInterfaceImplementationChanges = false)
            {
                return false;
            }

            internal override bool IsMetadataVirtual(bool ignoreInterfaceImplementationChanges = false)
            {
                return false;
            }

            internal override bool IsMetadataFinal()
            {
                return false;
            }

            public override MethodKind MethodKind
            {
                get { return MethodKind.Ordinary; }
            }

            public override int Arity
            {
                get { return 0; }
            }

            public override bool IsExtensionMethod
            {
                get { return false; }
            }

            internal override bool HasSpecialName
            {
                get { return false; }
            }

            internal override System.Reflection.MethodImplAttributes ImplementationAttributes
            {
                get { return default(System.Reflection.MethodImplAttributes); }
            }

            internal override bool HasDeclarativeSecurity
            {
                get { return false; }
            }

            public override DllImportData GetDllImportData()
            {
                return null;
            }

            internal override IEnumerable<Cci.SecurityAttribute> GetSecurityInformation()
            {
                throw ExceptionUtilities.Unreachable;
            }

            internal override MarshalPseudoCustomAttributeData ReturnValueMarshallingInformation
            {
                get { return null; }
            }

            internal override bool RequiresSecurityObject
            {
                get { return false; }
            }

            public override bool HidesBaseMethodsByName
            {
                get { return false; }
            }

            public override bool IsVararg
            {
                get { return false; }
            }

            public override bool ReturnsVoid
            {
                get { return true; }
            }

            public override bool IsAsync
            {
                get { return false; }
            }

            public override TypeSymbol ReturnType
            {
                get { return ContainingAssembly.GetSpecialType(SpecialType.System_Void); }
            }

            public override ImmutableArray<TypeSymbol> TypeArguments
            {
                get { return ImmutableArray<TypeSymbol>.Empty; }
            }

            public override ImmutableArray<TypeParameterSymbol> TypeParameters
            {
                get { return ImmutableArray<TypeParameterSymbol>.Empty; }
            }

            public override ImmutableArray<ParameterSymbol> Parameters
            {
                get { return ImmutableArray<ParameterSymbol>.Empty; }
            }

            public override ImmutableArray<MethodSymbol> ExplicitInterfaceImplementations
            {
                get { return ImmutableArray<MethodSymbol>.Empty; }
            }

            public override ImmutableArray<CustomModifier> ReturnTypeCustomModifiers
            {
                get { return ImmutableArray<CustomModifier>.Empty; }
            }

            public override Symbol AssociatedPropertyOrEvent
            {
                get { return null; }
            }

            internal override ImmutableArray<string> GetAppliedConditionalSymbols()
            {
                return ImmutableArray<string>.Empty;
            }

            internal override Cci.CallingConvention CallingConvention
            {
                get { return Cci.CallingConvention.HasThis; }
            }

            internal override bool GenerateDebugInfo
            {
                get { return true; }
            }

            public override Symbol ContainingSymbol
            {
                get { return containingType; }
            }

            public override ImmutableArray<Location> Locations
            {
                get { return ContainingType.Locations; }
            }

            public override Accessibility DeclaredAccessibility
            {
                get { return Accessibility.Private; }
            }

            public override bool IsStatic
            {
                get { return false; }
            }

            public override bool IsVirtual
            {
                get { return false; }
            }

            public override bool IsOverride
            {
                get { return false; }
            }

            public override bool IsAbstract
            {
                get { return false; }
            }

            public override bool IsSealed
            {
                get { return false; }
            }

            public override bool IsExtern
            {
                get { return false; }
            }
        }
    }
}
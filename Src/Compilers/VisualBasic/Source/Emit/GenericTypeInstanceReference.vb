﻿' Copyright (c) Microsoft Open Technologies, Inc.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text
Imports Microsoft.CodeAnalysis.Text
Imports Microsoft.CodeAnalysis.VisualBasic.Symbols
Imports Microsoft.CodeAnalysis.VisualBasic.Syntax

Namespace Microsoft.CodeAnalysis.VisualBasic.Emit

    ''' <summary>
    ''' Represents a reference to a generic type instantiation.
    ''' Subclasses represent nested and namespace types.
    ''' </summary>
    Friend MustInherit Class GenericTypeInstanceReference
        Inherits NamedTypeReference
        Implements Microsoft.Cci.IGenericTypeInstanceReference

        Public Sub New(underlyingNamedType As NamedTypeSymbol)
            MyBase.New(underlyingNamedType)
        End Sub

        Public NotOverridable Overrides Sub Dispatch(visitor As Microsoft.Cci.MetadataVisitor)
            visitor.Visit(DirectCast(Me, Microsoft.Cci.IGenericTypeInstanceReference))
        End Sub

        Private Function IGenericTypeInstanceReferenceGetGenericArguments(context As Microsoft.CodeAnalysis.Emit.Context) As IEnumerable(Of Microsoft.Cci.ITypeReference) Implements Microsoft.Cci.IGenericTypeInstanceReference.GetGenericArguments
            Dim moduleBeingBuilt As PEModuleBuilder = DirectCast(context.Module, PEModuleBuilder)

            Return From type In m_UnderlyingNamedType.TypeArgumentsNoUseSiteDiagnostics
                   Select moduleBeingBuilt.Translate(type, syntaxNodeOpt:=DirectCast(context.SyntaxNodeOpt, VisualBasicSyntaxNode), diagnostics:=context.Diagnostics)
        End Function

        Private ReadOnly Property IGenericTypeInstanceReferenceGenericType As Microsoft.Cci.INamedTypeReference Implements Microsoft.Cci.IGenericTypeInstanceReference.GenericType
            Get
                Debug.Assert(m_UnderlyingNamedType.OriginalDefinition Is m_UnderlyingNamedType.OriginalDefinition.OriginalDefinition)
                Return m_UnderlyingNamedType.OriginalDefinition
            End Get
        End Property
    End Class
End Namespace

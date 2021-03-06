﻿' Copyright (c) Microsoft Open Technologies, Inc.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports System.Collections.Immutable
Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Text
Imports Microsoft.CodeAnalysis.Test.Utilities
Imports Microsoft.CodeAnalysis.VisualBasic
Imports Microsoft.CodeAnalysis.VisualBasic.Symbols
Imports Microsoft.CodeAnalysis.VisualBasic.Syntax
Imports VBReferenceManager = Microsoft.CodeAnalysis.VisualBasic.VisualBasicCompilation.ReferenceManager

Module MetadataTestHelpers

    <Extension()>
    Friend Function GetCorLibType(this As ModuleSymbol, typeId As SpecialType) As NamedTypeSymbol
        Return this.ContainingAssembly.GetSpecialType(typeId)
    End Function

    <Extension()>
    Friend Function CorLibrary(this As ModuleSymbol) As AssemblySymbol
        Return this.ContainingAssembly.CorLibrary
    End Function

    Friend Function LoadFromBytes(bytes() As Byte) As AssemblySymbol

        Using MetadataCache.LockAndClean()
            Dim retval = GetSymbolsForReferences({bytes})(0)
            Return retval
        End Using

    End Function

    Private Class AssemblyInfoHelper : Inherits MarshalByRefObject
        Public Function GetAssemblyFullName(bytes() As Byte) As String
            Return Assembly.Load(bytes).FullName
        End Function
    End Class

    Friend Function GetSymbolsForReferences(
        references As Object(),
        Optional importInternals As Boolean = False
    ) As AssemblySymbol()

        Dim refs As New List(Of MetadataReference)

        For Each r In references
            Dim path = TryCast(r, String)
            If path IsNot Nothing Then
                refs.Add(New MetadataFileReference(path))
                Continue For
            End If

            Dim bytes = TryCast(r, Byte())
            If bytes IsNot Nothing Then
                refs.Add(New MetadataImageReference(bytes.AsImmutableOrNull()))
                Continue For
            End If

            If TypeOf r Is ImmutableArray(Of Byte) Then
                refs.Add(New MetadataImageReference(CType(r, ImmutableArray(Of Byte))))
                Continue For
            End If

            Dim c = TryCast(r, VisualBasicCompilation)
            If c IsNot Nothing Then
                refs.Add(New VisualBasicCompilationReference(c))
                Continue For
            End If

            Dim m = TryCast(r, MetadataReference)
            If m IsNot Nothing Then
                refs.Add(m)
                Continue For
            End If

            Throw TestExceptionUtilities.Unreachable
        Next

        Dim options = If(importInternals, OptionsDllAlwaysImportInternals, OptionsDll)
        Dim tc1 = VisualBasicCompilation.Create("Dummy", references:=refs, options:=options)

        Return (From ref In refs Select tc1.GetReferencedAssemblySymbol(ref)).ToArray()
    End Function

End Module

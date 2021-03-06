﻿' Copyright (c) Microsoft Open Technologies, Inc.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports System.Collections.Generic
Imports System.Linq
Imports Microsoft.CodeAnalysis.Text
Imports Microsoft.CodeAnalysis.VisualBasic.Symbols
Imports Microsoft.CodeAnalysis.VisualBasic.Syntax

Namespace Microsoft.CodeAnalysis.VisualBasic

    ''' <summary>
    ''' A region analysis walker that records jumps into the region.  Works by overriding NoteBranch, which is
    ''' invoked by a superclass when the two endpoints of a jump have been identified.
    ''' </summary>
    ''' <remarks></remarks>
    Class EntryPointsWalker
        Inherits AbstractRegionControlFlowPass

        Friend Overloads Shared Function Analyze(info As FlowAnalysisInfo, region As FlowAnalysisRegionInfo, ByRef succeeded As Boolean?) As IEnumerable(Of LabelStatementSyntax)
            Dim walker = New EntryPointsWalker(info, region)
            Try
                succeeded = walker.Analyze()
                Return If(succeeded, walker.entryPoints, Enumerable.Empty(Of LabelStatementSyntax)())
            Finally
                walker.Free()
            End Try
        End Function

        Dim entryPoints As HashSet(Of LabelStatementSyntax) = New HashSet(Of LabelStatementSyntax)()

        Private Overloads Function Analyze() As Boolean
            '  We only need to scan in a single pass.
            Return Scan()
        End Function

        Private Sub New(info As FlowAnalysisInfo, region As FlowAnalysisRegionInfo)
            MyBase.New(info, region)
        End Sub

        Protected Overrides Sub Free()
            MyBase.Free()
        End Sub

        Protected Overrides Sub NoteBranch(pending As PendingBranch, stmt As BoundStatement, labelStmt As BoundLabelStatement)
            If stmt.Syntax IsNot Nothing AndAlso labelStmt.Syntax IsNot Nothing AndAlso IsInsideRegion(labelStmt.Syntax.Span) AndAlso Not IsInsideRegion(stmt.Syntax.Span) Then
                Select Case stmt.Kind
                    Case BoundKind.GotoStatement
                        entryPoints.Add(DirectCast(labelStmt.Syntax, LabelStatementSyntax))

                    Case BoundKind.ReturnStatement
                        ' Do nothing

                    Case Else
                        Throw ExceptionUtilities.UnexpectedValue(stmt.Kind)
                End Select
            End If
        End Sub

    End Class

End Namespace

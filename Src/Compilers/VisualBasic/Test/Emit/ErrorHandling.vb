﻿' Copyright (c) Microsoft Open Technologies, Inc.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.Text
Imports Microsoft.CodeAnalysis.VisualBasic
Imports Microsoft.CodeAnalysis.VisualBasic.Symbols
Imports Microsoft.CodeAnalysis.VisualBasic.Syntax
Imports Microsoft.CodeAnalysis.VisualBasic.UnitTests.Emit
Imports Roslyn.Test.Utilities
Imports Xunit

Namespace Microsoft.CodeAnalysis.VisualBasic.UnitTests

    Public Class ErrorHandlingTests
        Inherits BasicTestBase

        <Fact()>
        Sub ErrorHandler_WithValidLabel_No_Resume()
            Dim compilationDef =
    <compilation>
        <file name="a.vb">
Imports System
Module Module1
    Sub Main()
        Dim sPath As String = ""
        sPath = "Test1"
        On Error GoTo foo
        Error 5
        Console.WriteLine(sPath)
        Exit Sub
foo:
        sPath &amp;= "foo"
        Console.WriteLine(sPath)
    End Sub
End Module
</file>
    </compilation>

            Dim compilation = CompilationUtils.CreateCompilationWithMscorlibAndVBRuntime(compilationDef)
            CompileAndVerify(compilation)
        End Sub

        <Fact()>
        Sub ErrorHandler_WithGotoMinus1andMatchingLabel()
            Dim compilationDef =
    <compilation>
        <file name="a.vb">
Imports System

Module Module1    
    Public Sub Main()        
        On Error GoTo -1
        Error 5
        exit sub
foo:
        Resume 
    End Sub
End Module 
</file>
    </compilation>

            Dim compilation = CompilationUtils.CreateCompilationWithMscorlibAndVBRuntime(compilationDef)
            CompileAndVerify(compilation)
        End Sub

        <Fact()>
        Sub Error_ErrorHandler_WithGoto0andNoMatchingLabel()
            Dim compilationDef =
    <compilation>
        <file name="a.vb">
Imports System

Module Module1    
    Public Sub Main()
        On Error GoTo 0
        Error 5
        exit sub
Foo:
        Resume 
    End Sub
End Module 
</file>
    </compilation>

            Dim compilation = CompilationUtils.CreateCompilationWithMscorlibAndVBRuntime(compilationDef)
            CompileAndVerify(compilation)
            'compilation.VerifyDiagnostics()
        End Sub

        <Fact()>
        Sub Error_ErrorHandler_WithResumeNext()
            Dim compilationDef =
    <compilation>
        <file name="a.vb">
Imports System

Module Module1    
    Public Sub Main()
        On Error Resume Next
        Error 5
        exit sub
    End Sub
End Module 
</file>
    </compilation>

            Dim compilation = CompilationUtils.CreateCompilationWithMscorlibAndVBRuntime(compilationDef)
            CompileAndVerify(compilation)
            'compilation.VerifyDiagnostics()
        End Sub

        <Fact()>
        Sub ErrorHandler_WithValidLabelMatchingKeywordsEscaped()
            Dim compilationDef =
    <compilation>
        <file name="a.vb">
Imports System

Module Module1    
    Public Sub Main()        
        On Error GoTo [On]
        On Error GoTo [goto]  'Doesn't matter if case mismatch (Didn't pretty list correctly)
        On Error GoTo [Error]

        exit sub
[Goto]:
        Resume [on]

[On]:
        Resume [Error]

[Error]:
        Resume [Goto]
    End Sub
End Module 
</file>
    </compilation>

            Dim compilation = CompilationUtils.CreateCompilationWithMscorlibAndVBRuntime(compilationDef)
            CompileAndVerify(compilation)
            ' compilation.VerifyDiagnostics()
        End Sub

        <Fact()>
        Sub Error_ErrorHandler_WithValidLabelMatchingKeywordsNotEscaped()
            Dim compilationDef =
    <compilation>
        <file name="a.vb">
    Module Module1    
        Public Sub Main()        
            On Error GoTo On
            On Error GoTo goto  
            On Error GoTo Error

    [Goto]:
            Resume [on]

    [On]:
            Resume [Error]

    [Error]:
            Resume [Goto]
        End Sub
    End Module 
    </file>
    </compilation>

            Dim compilation = CompilationUtils.CreateCompilationWithMscorlibAndVBRuntime(compilationDef)

            compilation.VerifyDiagnostics(Diagnostic(ERRID.ERR_ExpectedIdentifier, ""),
        Diagnostic(ERRID.ERR_ExpectedIdentifier, ""),
        Diagnostic(ERRID.ERR_ExpectedIdentifier, ""),
        Diagnostic(ERRID.ERR_LabelNotDefined1, "").WithArguments(""),
        Diagnostic(ERRID.ERR_LabelNotDefined1, "").WithArguments(""),
        Diagnostic(ERRID.ERR_LabelNotDefined1, "").WithArguments(""))
        End Sub


        <Fact()>
        Sub Error_ErrorHandler_WithInValidLabelMatchingKeywordsEscaped()
            Dim compilationDef =
    <compilation>
        <file name="a.vb">
    Module Module1    
        Public Sub Main()        
            On Error GoTo [On]
            On Error GoTo [goto]  'Doesn't matter if case mismatch (Didn't pretty list correctly)
            On Error GoTo [Error]

    Goto:
            Resume on

    On:
            Resume Error

    Error:
            Resume Goto
        End Sub
    End Module 
    </file>
    </compilation>

            Dim compilation = CompilationUtils.CreateCompilationWithMscorlibAndVBRuntime(compilationDef)

            compilation.VerifyDiagnostics(Diagnostic(ERRID.ERR_ExpectedIdentifier, ""),
        Diagnostic(ERRID.ERR_ExpectedIdentifier, ""),
        Diagnostic(ERRID.ERR_ObsoleteOnGotoGosub, ""),
        Diagnostic(ERRID.ERR_ExpectedIdentifier, ""),
        Diagnostic(ERRID.ERR_ExpectedExpression, ""),
        Diagnostic(ERRID.ERR_ExpectedIdentifier, ""),
        Diagnostic(ERRID.ERR_LabelNotDefined1, "[On]").WithArguments("On"),
        Diagnostic(ERRID.ERR_LabelNotDefined1, "[goto]").WithArguments("goto"),
        Diagnostic(ERRID.ERR_LabelNotDefined1, "[Error]").WithArguments("Error"),
        Diagnostic(ERRID.ERR_LabelNotDefined1, "").WithArguments(""),
        Diagnostic(ERRID.ERR_LabelNotDefined1, "").WithArguments(""),
        Diagnostic(ERRID.ERR_LabelNotDefined1, "").WithArguments(""),
        Diagnostic(ERRID.ERR_LabelNotDefined1, "").WithArguments(""),
        Diagnostic(ERRID.ERR_LabelNotDefined1, "").WithArguments(""))
        End Sub

        <Fact()>
        Sub Error_ErrorHandler_WithGoto0andMatchingLabel()
            Dim compilationDef =
    <compilation>
        <file name="a.vb">
    Imports System

    Module Module1    
        Public Sub Main()
            On Error GoTo 0
            Error 5
            exit sub
    0:
            Resume 
        End Sub
    End Module 
    </file>
    </compilation>

            Dim compilation = CompilationUtils.CreateCompilationWithMscorlibAndVBRuntime(compilationDef)

            Dim compilationVerifier = CompileAndVerify(compilation)
            compilationVerifier.VerifyIL("Module1.Main", <![CDATA[
    {
      // Code size      155 (0x9b)
      .maxstack  3
      .locals init (Integer V_0, //VB$ActiveHandler
      Integer V_1, //VB$ResumeTarget
      Integer V_2, //VB$CurrentStatement
      Integer V_3) //VB$CurrentLine
      .try
    {
      IL_0000:  call       "Sub Microsoft.VisualBasic.CompilerServices.ProjectData.ClearProjectError()"
      IL_0005:  ldc.i4.0
      IL_0006:  stloc.0
      IL_0007:  ldc.i4.2
      IL_0008:  stloc.2
      IL_0009:  ldc.i4.5
      IL_000a:  call       "Function Microsoft.VisualBasic.CompilerServices.ProjectData.CreateProjectError(Integer) As System.Exception"
      IL_000f:  throw
      IL_0010:  ldc.i4.0
      IL_0011:  stloc.3
      IL_0012:  ldc.i4.5
      IL_0013:  stloc.2
      IL_0014:  call       "Sub Microsoft.VisualBasic.CompilerServices.ProjectData.ClearProjectError()"
      IL_0019:  ldloc.1
      IL_001a:  brtrue.s   IL_0029
      IL_001c:  ldc.i4     0x800a0014
      IL_0021:  call       "Function Microsoft.VisualBasic.CompilerServices.ProjectData.CreateProjectError(Integer) As System.Exception"
      IL_0026:  throw
      IL_0027:  leave.s    IL_0092
      IL_0029:  ldloc.1
      IL_002a:  br.s       IL_002f
      IL_002c:  ldloc.1
      IL_002d:  ldc.i4.1
      IL_002e:  add
      IL_002f:  ldc.i4.0
      IL_0030:  stloc.1
      IL_0031:  switch    (
      IL_0052,
      IL_0000,
      IL_0007,
      IL_0027,
      IL_0010,
      IL_0012,
      IL_0027)
      IL_0052:  leave.s    IL_0087
      IL_0054:  ldloc.2
      IL_0055:  stloc.1
      IL_0056:  ldloc.0
      IL_0057:  switch    (
      IL_0064,
      IL_002c)
      IL_0064:  leave.s    IL_0087
    }
      filter
    {
      IL_0066:  isinst     "System.Exception"
      IL_006b:  ldnull
      IL_006c:  cgt.un
      IL_006e:  ldloc.0
      IL_006f:  ldc.i4.0
      IL_0070:  cgt.un
      IL_0072:  and
      IL_0073:  ldloc.1
      IL_0074:  ldc.i4.0
      IL_0075:  ceq
      IL_0077:  and
      IL_0078:  endfilter
    }  // end filter
    {  // handler
      IL_007a:  castclass  "System.Exception"
      IL_007f:  ldloc.3
      IL_0080:  call       "Sub Microsoft.VisualBasic.CompilerServices.ProjectData.SetProjectError(System.Exception, Integer)"
      IL_0085:  leave.s    IL_0054
    }
      IL_0087:  ldc.i4     0x800a0033
      IL_008c:  call       "Function Microsoft.VisualBasic.CompilerServices.ProjectData.CreateProjectError(Integer) As System.Exception"
      IL_0091:  throw
      IL_0092:  ldloc.1
      IL_0093:  brfalse.s  IL_009a
      IL_0095:  call       "Sub Microsoft.VisualBasic.CompilerServices.ProjectData.ClearProjectError()"
      IL_009a:  ret
    }
    ]]>)
        End Sub

        <Fact()>
        Sub Error_ErrorHandler_WithGoto1andMatchingLabel()
            Dim compilationDef =
    <compilation>
        <file name="a.vb">
    Imports System

    Module Module1    
        Public Sub Main()        
            On Error GoTo 1
            Console.writeline("Start")        
            Error 5
            Console.writeline("2")
            exit sub
    1:
    Console.writeline("1")
            Resume Next
        End Sub
    End Module 
    </file>
    </compilation>


            Dim compilation = CreateCompilationWithMscorlibAndVBRuntime(compilationDef, OptionsExe.WithDebugInformationKind(DebugInformationKind.None).WithOptimizations(True))
            CompileAndVerify(compilation, expectedOutput:=<![CDATA[Start
1
2]]>)
        End Sub

        <Fact()>
        Sub Error_ErrorHandler_WithMissingOrIncorrectLabels()
            Dim compilationDef =
    <compilation>
        <file name="a.vb">
    Module Module1   
        sing Labels
        Public Sub Main()           
        End Sub

        Sub Goto_MissingLabel()
            'Error - label is not present
            On Error GoTo foo

        End Sub

        Sub GotoLabelInDifferentMethod()
            'Error - no label in this method, in a different so it will fail
            On Error GoTo diffMethodLabel
        End Sub

        Sub GotoLabelInDifferentMethod()
            'Error - no label in this method - trying to fully qualify will fail
            On Error GoTo DifferentMethod.diffMethodLabel
        End Sub

        Sub DifferentMethod()
    DiffMethodLabel:
        End Sub
    End Module 
    </file>
    </compilation>


            Dim compilation = CreateCompilationWithMscorlibAndVBRuntime(compilationDef, OptionsExe.WithDebugInformationKind(DebugInformationKind.None).WithOptimizations(True))
            compilation.VerifyDiagnostics(Diagnostic(ERRID.ERR_ExpectedSpecifier, "Labels"),
                                          Diagnostic(ERRID.ERR_ExpectedDeclaration, "sing"),
                                          Diagnostic(ERRID.ERR_ExpectedEOS, "."),
                                          Diagnostic(ERRID.ERR_DuplicateProcDef1, "GotoLabelInDifferentMethod").WithArguments("Public Sub GotoLabelInDifferentMethod()"),
                                          Diagnostic(ERRID.ERR_LabelNotDefined1, "foo").WithArguments("foo"),
                                          Diagnostic(ERRID.ERR_LabelNotDefined1, "diffMethodLabel").WithArguments("diffMethodLabel"),
                                          Diagnostic(ERRID.ERR_LabelNotDefined1, "DifferentMethod").WithArguments("DifferentMethod"))
        End Sub



        <Fact()>
        Sub Error_ErrorHandler_BothTypesOfErrorHandling()
            Dim compilationDef =
    <compilation>
        <file name="a.vb">
    Imports System

    Module Module1   
        Sub Main
            TryAndOnErrorInSameMethod
            OnErrorAndTryInSameMethod
        End Sub

        Sub TryAndOnErrorInSameMethod()
            'Nested
            Try
                On Error GoTo foo
    foo:
            Catch ex As Exception
            End Try
        End Sub

        Sub OnErrorAndTryInSameMethod()
            'Sequential
            On Error GoTo foo
    foo:
            Try
            Catch ex As Exception
            End Try
        End Sub
    End Module 
    </file>
    </compilation>

            Dim compilation = CreateCompilationWithMscorlibAndVBRuntime(compilationDef, OptionsExe.WithDebugInformationKind(DebugInformationKind.None).WithOptimizations(True))

            Dim ExpectedOutput = <![CDATA[Try
                On Error GoTo foo
    foo:
            Catch ex As Exception
            End Try]]>

            Dim ExpectedOutput2 = <![CDATA[Try
            Catch ex As Exception
            End Try]]>

            compilation.VerifyDiagnostics(Diagnostic(ERRID.ERR_TryAndOnErrorDoNotMix, ExpectedOutput),
                                          Diagnostic(ERRID.ERR_TryAndOnErrorDoNotMix, "On Error GoTo foo"),
                                          Diagnostic(ERRID.ERR_TryAndOnErrorDoNotMix, "On Error GoTo foo"),
                                          Diagnostic(ERRID.ERR_TryAndOnErrorDoNotMix, ExpectedOutput2)
    )
        End Sub

        <Fact()>
        Sub Error_ErrorHandler_InVBCore()
            'Old Style handling not supported in VBCore
            Dim compilationDef =
    <compilation>
        <file name="a.vb">
    Module Module1   
        Public Sub Main        
            On Error GoTo foo
    foo:
        End Sub
    End Module 
    </file>
    </compilation>

            Dim compilation = CompilationUtils.CreateCompilationWithReferences(compilationDef,
                                                                         references:={MscorlibRef, SystemRef, SystemCoreRef},
                                                                         options:=OptionsDll.WithEmbedVbCoreRuntime(True))

            Dim ExpectedOutput = <![CDATA[Public Sub Main        
            On Error GoTo foo
    foo:
        End Sub]]>

            compilation.VerifyDiagnostics(Diagnostic(ERRID.ERR_MissingRuntimeHelper, ExpectedOutput).WithArguments("Microsoft.VisualBasic.CompilerServices.ProjectData.CreateProjectError"))
        End Sub

        <Fact()>
        Sub Error_ErrorHandler_OutsideOfMethodBody()
            Dim compilationDef =
    <compilation>
        <file name="a.vb">
    Module Module1   
        Sub Main

        End Sub

        'Error Outside of Method Body
        On Error Goto foo

        Sub Foo
        End Sub
    End Module 
    </file>
    </compilation>

            Dim compilation = CreateCompilationWithMscorlibAndVBRuntime(compilationDef, OptionsExe.WithDebugInformationKind(DebugInformationKind.None).WithOptimizations(True))
            compilation.VerifyDiagnostics(Diagnostic(ERRID.ERR_ExecutableAsDeclaration, "On Error Goto foo"))
        End Sub

        <Fact()>
        Sub ErrorHandler_In_Different_Types()
            'Basic Validation that this is permissible in Class/Structure/(Module Tested elsewhere)
            'Generic
            Dim compilationDef =
    <compilation>
        <file name="a.vb">
    Imports System

    Module Module1   
        Sub Main        
        End Sub

      Class Foo
            Sub Method()
                On Error GoTo CLassMethodLabel

    CLassMethodLabel:
            End Sub

            Public Property ABC As String
                Set(value As String)
                    On Error GoTo setLabel
    setLabel:
                End Set
                Get
                    On Error GoTo getLabel
    getLabel:

                End Get
            End Property
        End Class


    Structure Foo_Struct
            Sub Method()
                On Error GoTo StructMethodLabel

    StructMethodLabel:
            End Sub

            Public Property ABC As String
                Set(value As String)
                    On Error GoTo SetLabel
    setLabel:
                End Set
                Get
                    On Error GoTo getLabel
    getLabel:

                End Get
            End Property
        End Structure 

        Class GenericFoo(Of t)
            Sub Method()
                'Normal Method In Generic Class
                On Error GoTo CLassMethodLabel

    CLassMethodLabel:

            End Sub

            Sub GenericMethod(Of u)(x As u)
                'Generic Method In Generic Class
                On Error GoTo CLassMethodLabel

    CLassMethodLabel:

            End Sub
            Public Property ABC As String
                Set(value As String)
                    On Error GoTo setLabel
    setLabel:
                End Set
                Get
                    On Error GoTo getLabel
    getLabel:

                End Get
            End Property
        End Class
    End Module 
    </file>
    </compilation>

            Dim compilation = CreateCompilationWithMscorlibAndVBRuntime(compilationDef, OptionsExe.WithDebugInformationKind(DebugInformationKind.None).WithOptimizations(True))
            compilation.AssertNoDiagnostics()
        End Sub

        <Fact()>
        Sub ErrorHandler_Other_Constructor_Dispose()
            Dim compilationDef =
    <compilation>
        <file name="a.vb">
    Imports System

    Module Module1   
        Sub Main        
            Dim X As New TestInConstructor

            Dim X2 As New TestInDisposeAndFinalize
            X2.Dispose()
        End Sub
    End Module

        Class TestInConstructor
            Sub New()
                On Error GoTo constructorError
    ConstructorError:

            End Sub
        End Class

        Class TestInDisposeAndFinalize
            Implements IDisposable

            Sub New()
            End Sub

    #Region "IDisposable Support"
            Private disposedValue As Boolean ' To detect redundant calls

            ' IDisposable
            Protected Overridable Sub Dispose(disposing As Boolean)
                On Error GoTo ConstructorError

                If Not Me.disposedValue Then
                    If disposing Then
                        ' TODO: dispose managed state (managed objects).
                    End If

                    ' TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
                    ' TODO: set large fields to null.
                End If
                Me.disposedValue = True

    ConstructorError:

            End Sub

            ' TODO: override Finalize() only if Dispose(ByVal disposing As Boolean) above has code to free unmanaged resources.
            Protected Overrides Sub Finalize()
                On Error GoTo FInalizeError
                ' Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
                Dispose(False)
                MyBase.Finalize()
    FInalizeError:

            End Sub

            ' This code added by Visual Basic to correctly implement the disposable pattern.
            Public Sub Dispose() Implements IDisposable.Dispose
                ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
                Dispose(True)
                GC.SuppressFinalize(Me)
            End Sub
    #End Region
        End Class
    </file>
    </compilation>

            Dim compilation = CreateCompilationWithMscorlibAndVBRuntime(compilationDef, OptionsExe.WithDebugInformationKind(DebugInformationKind.None).WithOptimizations(True))
            compilation.VerifyDiagnostics()
        End Sub

        <Fact()>
        Sub Error_InvalidTypes_ImplicitConversions()

            Dim compilationDef =
    <compilation>
        <file name="a.vb">
    Imports System

    Module Module1   
        Sub Main        
            Error 1L    
            Error 2S
            Error &quot;3&quot;
            Error 4!      
            Error 5%
        End Sub
        End Module
    </file>
    </compilation>

            Dim compilation = CreateCompilationWithMscorlibAndVBRuntime(compilationDef, OptionsExe.WithDebugInformationKind(DebugInformationKind.None).WithOptimizations(True))
            compilation.AssertNoDiagnostics()
        End Sub

        <Fact()>
        Sub Error_InvalidTypes_InvalidTypes_StrictOn()
            Dim compilationDef =
    <compilation>
        <file name="a.vb">
    Option Strict On

    Module Module1   
        Sub Main        
            Error 1L    
            Error 2S
            Error &quot;3&quot;
            Error 4!      
            Error 5%
        End Sub
        End Module
    </file>
    </compilation>

            Dim compilation = CreateCompilationWithMscorlibAndVBRuntime(compilationDef, OptionsExe.WithDebugInformationKind(DebugInformationKind.None).WithOptimizations(True))
            compilation.VerifyDiagnostics(Diagnostic(ERRID.ERR_NarrowingConversionDisallowed2, """3""").WithArguments("String", "Integer"),
                                          Diagnostic(ERRID.ERR_NarrowingConversionDisallowed2, "4!").WithArguments("Single", "Integer"))
        End Sub

        <Fact()>
        Sub ErrorHandler_Error_InSyncLockBlock()
            Dim compilationDef =
    <compilation>
        <file name="a.vb">
    Class LockClass
    End Class

    Module Module1
        Sub Main()
            Dim lock As New LockClass

            On Error GoTo handler

            SyncLock lock
                On Error GoTo foo

    foo:
                Resume Next
            End SyncLock
            Exit Sub
        End Sub
    End Module
    </file>
    </compilation>

            Dim compilation = CreateCompilationWithMscorlibAndVBRuntime(compilationDef, OptionsExe.WithDebugInformationKind(DebugInformationKind.None).WithOptimizations(True))
            compilation.VerifyDiagnostics(Diagnostic(ERRID.ERR_LabelNotDefined1, "handler").WithArguments("handler"),
                                          Diagnostic(ERRID.ERR_OnErrorInSyncLock, "On Error GoTo foo"))
        End Sub

        <Fact()>
        Sub ErrorHandler_Error_InMethodWithSyncLockBlock()
            'Method has a Error Handler and Error Occurs within SyncLock
            'resume next will occur outside of the SyncLock Block
            Dim compilationDef =
    <compilation>
        <file name="a.vb">
    Imports System

    Class LockClass
    End Class

    Module Module1
        Sub Main()
            Dim lock As New LockClass
            Console.WriteLine("Start")
            On Error GoTo handler

            SyncLock lock
                Console.WriteLine("In SyncLock")
                Error 1
                Console.WriteLine("After Error In SyncLock")
            End SyncLock

            Console.WriteLine("End")
            Exit Sub
    handler:
            Console.WriteLine("Handler")
            Resume Next
        End Sub
    End Module
    </file>
    </compilation>

            Dim compilation = CreateCompilationWithMscorlibAndVBRuntime(compilationDef, OptionsExe.WithDebugInformationKind(DebugInformationKind.None).WithOptimizations(True))
            compilation.VerifyDiagnostics()
            Dim CompilationVerifier = CompileAndVerify(compilation, expectedOutput:=<![CDATA[Start
In SyncLock
Handler
End]]>)

            CompilationVerifier.VerifyIL("Module1.Main", <![CDATA[
{
  // Code size      251 (0xfb)
  .maxstack  3
  .locals init (Integer V_0, //VB$ActiveHandler
  Integer V_1, //VB$ResumeTarget
  Integer V_2, //VB$CurrentStatement
  LockClass V_3, //lock
  Object V_4, //VB$Lock
  Boolean V_5) //VB$LockTaken
  .try
{
  IL_0000:  ldc.i4.1
  IL_0001:  stloc.2
  IL_0002:  newobj     "Sub LockClass..ctor()"
  IL_0007:  stloc.3
  IL_0008:  ldc.i4.2
  IL_0009:  stloc.2
  IL_000a:  ldstr      "Start"
  IL_000f:  call       "Sub System.Console.WriteLine(String)"
  IL_0014:  call       "Sub Microsoft.VisualBasic.CompilerServices.ProjectData.ClearProjectError()"
  IL_0019:  ldc.i4.2
  IL_001a:  stloc.0
  IL_001b:  ldc.i4.4
  IL_001c:  stloc.2
  IL_001d:  ldloc.3
  IL_001e:  stloc.s    V_4
  IL_0020:  ldc.i4.0
  IL_0021:  stloc.s    V_5
  .try
{
  IL_0023:  ldloc.s    V_4
  IL_0025:  ldloca.s   V_5
  IL_0027:  call       "Sub System.Threading.Monitor.Enter(Object, ByRef Boolean)"
  IL_002c:  ldstr      "In SyncLock"
  IL_0031:  call       "Sub System.Console.WriteLine(String)"
  IL_0036:  ldc.i4.1
  IL_0037:  call       "Function Microsoft.VisualBasic.CompilerServices.ProjectData.CreateProjectError(Integer) As System.Exception"
  IL_003c:  throw
}
  finally
{
  IL_003d:  ldloc.s    V_5
  IL_003f:  brfalse.s  IL_0048
  IL_0041:  ldloc.s    V_4
  IL_0043:  call       "Sub System.Threading.Monitor.Exit(Object)"
  IL_0048:  endfinally
}
  IL_0049:  ldc.i4.5
  IL_004a:  stloc.2
  IL_004b:  ldstr      "End"
  IL_0050:  call       "Sub System.Console.WriteLine(String)"
  IL_0055:  leave      IL_00f2
  IL_005a:  ldc.i4.7
  IL_005b:  stloc.2
  IL_005c:  ldstr      "Handler"
  IL_0061:  call       "Sub System.Console.WriteLine(String)"
  IL_0066:  ldc.i4.8
  IL_0067:  stloc.2
  IL_0068:  call       "Sub Microsoft.VisualBasic.CompilerServices.ProjectData.ClearProjectError()"
  IL_006d:  ldloc.1
  IL_006e:  brtrue.s   IL_007d
  IL_0070:  ldc.i4     0x800a0014
  IL_0075:  call       "Function Microsoft.VisualBasic.CompilerServices.ProjectData.CreateProjectError(Integer) As System.Exception"
  IL_007a:  throw
  IL_007b:  leave.s    IL_00f2
  IL_007d:  ldloc.1
  IL_007e:  ldc.i4.1
  IL_007f:  add
  IL_0080:  ldc.i4.0
  IL_0081:  stloc.1
  IL_0082:  switch    (
  IL_00af,
  IL_0000,
  IL_0008,
  IL_0014,
  IL_001b,
  IL_0049,
  IL_007b,
  IL_005a,
  IL_0066,
  IL_007b)
  IL_00af:  leave.s    IL_00e7
  IL_00b1:  ldloc.2
  IL_00b2:  stloc.1
  IL_00b3:  ldloc.0
  IL_00b4:  switch    (
  IL_00c5,
  IL_007d,
  IL_005a)
  IL_00c5:  leave.s    IL_00e7
}
  filter
{
  IL_00c7:  isinst     "System.Exception"
  IL_00cc:  ldnull
  IL_00cd:  cgt.un
  IL_00cf:  ldloc.0
  IL_00d0:  ldc.i4.0
  IL_00d1:  cgt.un
  IL_00d3:  and
  IL_00d4:  ldloc.1
  IL_00d5:  ldc.i4.0
  IL_00d6:  ceq
  IL_00d8:  and
  IL_00d9:  endfilter
}  // end filter
{  // handler
  IL_00db:  castclass  "System.Exception"
  IL_00e0:  call       "Sub Microsoft.VisualBasic.CompilerServices.ProjectData.SetProjectError(System.Exception)"
  IL_00e5:  leave.s    IL_00b1
}
  IL_00e7:  ldc.i4     0x800a0033
  IL_00ec:  call       "Function Microsoft.VisualBasic.CompilerServices.ProjectData.CreateProjectError(Integer) As System.Exception"
  IL_00f1:  throw
  IL_00f2:  ldloc.1
  IL_00f3:  brfalse.s  IL_00fa
  IL_00f5:  call       "Sub Microsoft.VisualBasic.CompilerServices.ProjectData.ClearProjectError()"
  IL_00fa:  ret
}
]]>)
        End Sub

    End Class

End Namespace
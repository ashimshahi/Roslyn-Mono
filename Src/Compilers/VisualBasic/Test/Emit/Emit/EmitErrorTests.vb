﻿' Copyright (c) Microsoft Open Technologies, Inc.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

Imports System.IO
Imports Microsoft.CodeAnalysis
Imports Microsoft.CodeAnalysis.CodeGen
Imports Microsoft.CodeAnalysis.Test.Utilities
Imports Microsoft.CodeAnalysis.Text
Imports Microsoft.CodeAnalysis.VisualBasic
Imports Microsoft.CodeAnalysis.VisualBasic.Symbols
Imports Microsoft.CodeAnalysis.VisualBasic.Syntax
Imports Microsoft.CodeAnalysis.VisualBasic.UnitTests.Emit
Imports Roslyn.Test.Utilities
Imports Xunit

Namespace Microsoft.CodeAnalysis.VisualBasic.UnitTests

    ' this place is dedicated to emit/codegen related error tests
    Public Class EmitErrorTests
        Inherits BasicTestBase

#Region "Targeted Error Tests - please arrange tests in the order of error code"

        <Fact>
        Public Sub BC30297ERR_SubNewCycle2()
            Dim comp1 = CompilationUtils.CreateCompilationWithMscorlibAndVBRuntime(
    <compilation>
        <file name="a.vb">
Partial Class C1
    'COMPILEERROR: BC30297, "New"
    Sub New(ByVal a As Integer)
        MyClass.New()
    End Sub
End Class

Partial Class C1
    'COMPILEERROR: BC30297, "New"
    Sub New()
        MyClass.New(1)
    End Sub
End Class
        </file>
    </compilation>)
            comp1.AssertTheseDiagnostics(<errors>
BC30298: Constructor 'Public Sub New(a As Integer)' cannot call itself: 
    'Public Sub New(a As Integer)' calls 'Public Sub New()'.
    'Public Sub New()' calls 'Public Sub New(a As Integer)'.
    Sub New(ByVal a As Integer)
        ~~~
BC30298: Constructor 'Public Sub New()' cannot call itself: 
    'Public Sub New()' calls 'Public Sub New(a As Integer)'.
    'Public Sub New(a As Integer)' calls 'Public Sub New()'.
    Sub New()
        ~~~
     </errors>)
        End Sub

        <Fact()>
        Public Sub BC31522ERR_DllImportOnNonEmptySubOrFunction()
            Dim comp1 = CompilationUtils.CreateCompilationWithMscorlibAndReferences(
    <compilation>
        <file name="a.vb">
Module MyModule
    'COMPILEERROR: BC31522, "DllImport3_Label_NotEmpty" 
    &lt;System.Runtime.InteropServices.DllImport("somedll.dll")&gt; Public Sub DllImport3_Label_NotEmpty()
Label:
    End Sub
End Module
        </file>
    </compilation>, {SystemCoreRef})
            Dim executableStream = New MemoryStream()
            Dim pdbStream = New MemoryStream()
            Dim result = comp1.Emit(executableStream, Nothing, "test.pdb", pdbStream)
            Assert.Equal(1, result.Diagnostics.Where(Function(x) x.Code = 31522).Count())
        End Sub

        <Fact()>
        Public Sub BC32304ERR_ConflictDefaultPropertyAttribute()
            Dim comp1 = CompilationUtils.CreateCompilationWithMscorlib(
<compilation>
    <file name="a.vb"><![CDATA[
Imports System.Reflection
' Property with no parameters.
<DefaultMember("P")>
Class A
    Property P
End Class
' Different property (no parameters).
<DefaultMember("P")>
Class B
    Property P
    Default WriteOnly Property Q(o As Object)
        Set(value)
        End Set
    End Property
End Class
' Different case.
<DefaultMember("P")>
Class C
    Default ReadOnly Property p(o As Object)
        Get
            Return Nothing
        End Get
    End Property
End Class
' Different case.
<DefaultMember("p")>
Class D
    Default ReadOnly Property P(o As Object)
        Get
            Return Nothing
        End Get
    End Property
End Class
' Nothing DefaultMember value.
<DefaultMember(Nothing)>
Class E
    Default ReadOnly Property P(o As Object)
        Get
            Return Nothing
        End Get
    End Property
End Class
' Empty DefaultMember value.
<DefaultMember("")>
Class F
    Default ReadOnly Property P(o As Object)
        Get
            Return Nothing
        End Get
    End Property
End Class
' Different property (missing).
<DefaultMember("P")>
Structure S
    Default ReadOnly Property Q(o As Object)
        Get
            Return Nothing
        End Get
    End Property
End Structure
' Different member.
<DefaultMember("M")>
Interface I
    Sub M(o As Object)
    Default Property P(o As Object)
End Interface
]]></file>
</compilation>)
            comp1.AssertTheseDiagnostics(<errors><![CDATA[
BC32304: Conflict between the default property and the 'DefaultMemberAttribute' defined on 'B'.
Class B
      ~
BC32304: Conflict between the default property and the 'DefaultMemberAttribute' defined on 'E'.
Class E
      ~
BC32304: Conflict between the default property and the 'DefaultMemberAttribute' defined on 'F'.
Class F
      ~
BC32304: Conflict between the default property and the 'DefaultMemberAttribute' defined on 'S'.
Structure S
          ~
BC32304: Conflict between the default property and the 'DefaultMemberAttribute' defined on 'I'.
Interface I
          ~
]]></errors>)
        End Sub

        <WorkItem(540610)>
        <Fact>
        Public Sub OverrideProperty()
            CompileAndVerify(
    <compilation>
        <file name="a.vb">
Imports System
Module ORDefault001mod
    Class Base
        Public str As String
        Overridable WriteOnly Property myprop() As String
            Set(ByVal Value As String)
                str = Value
            End Set
        End Property
    End Class
    Class Derived
        Inherits Base
        Public stri As String
        Overrides WriteOnly Property myprop() As String
            Set(ByVal Value As String)
                stri = Value 
            End Set
        End Property
    End Class
    Sub Main()
        Dim cls2 As Derived = New Derived()
        cls2.myprop = "Hai"
        
    End Sub
End Module
        </file>
    </compilation>)
        End Sub

        <WorkItem(540577)>
        <Fact>
        Public Sub AddErrorCompRef()
            Dim comp1 = CreateCompilationWithMscorlib(
    <compilation>
        <file name="a.vb">
Public Class 
End Class
        </file>
    </compilation>)

            Dim comp2 = CreateCompilationWithMscorlibAndVBRuntime(
    <compilation>
        <file name="a.vb">
Module M1
Sub Main()
End Sub
End Module
        </file>
    </compilation>, additionalRefs:={New VisualBasicCompilationReference(comp1)})

            comp2.VerifyDiagnostics()
        End Sub

        <WorkItem(540643)>
        <Fact>
        Public Sub PEVerifyOverrides()
            Dim comp1 = CompilationUtils.CreateCompilationWithMscorlib(
    <compilation>
        <file name="a.vb">
Imports System
MustInherit Class cls1
    MustOverride Sub Foo()
End Class

Class cls2
    Inherits cls1
    Overrides Sub foo()
    End Sub
    Shared Sub Main()
    End Sub
End Class
        </file>
    </compilation>)

            CompileAndVerify(comp1)

        End Sub

        <WorkItem(6983, "DevDiv_Projects/Roslyn")>
        <Fact>
        Public Sub ConstCharWithChrw()
            Dim comp1 = CompilationUtils.CreateCompilationWithMscorlibAndVBRuntime(
    <compilation>
        <file name="a.vb">
Imports System
Imports Microsoft.VisualBasic
Imports Microsoft.VisualBasic.Constants
Imports Microsoft.VisualBasic.Conversion
Imports Microsoft.VisualBasic.DateAndTime
Imports Microsoft.VisualBasic.Information
Imports Microsoft.VisualBasic.Interaction
Imports Microsoft.VisualBasic.Strings
Imports Microsoft.VisualBasic.VBMath
Module M1
    Class C1
        Public Const x As Char = ChrW(97)
    End Class

    Sub Main()
        Dim y As C1 = New C1()
        Console.Write(y.x)
    End Sub
End Module
        </file>
    </compilation>)

            CompileAndVerify(comp1)

        End Sub

        <WorkItem(540536)>
        <Fact>
        Public Sub SkipCodeGenIfErrorExist()
            Dim comp1 = CompilationUtils.CreateCompilationWithMscorlibAndVBRuntime(
    <compilation>
        <file name="a.vb">
Option Explicit
Module M1
    Sub Main()
        Exit Sub
    End Sub
    Overloads Sub foo(ByVal arg As Byte)
    End Sub
    Overloads Sub foo(ByVal arg As System.Byte)
    End Sub
End Module
        </file>
    </compilation>)
            Dim executableStream = New MemoryStream()
            Dim pdbStream = New MemoryStream()
            Dim result = comp1.Emit(executableStream, Nothing, "test.pdb", pdbStream)
        End Sub

        <WorkItem(6999, "DevDiv_Projects/Roslyn")>
        <Fact>
        Public Sub LambdaForMultiLine()
            Dim comp1 = CompilationUtils.CreateCompilationWithMscorlibAndVBRuntime(
    <compilation>
        <file name="a.vb">
Module Module1
    Dim factorial As System.Func(Of Integer, Object) = Function(i As Integer)
                                                    System.Console.WriteLine("Called lambda.")
                                                    Return factorial
                                                End Function(1)

    Sub Main()
        System.Console.WriteLine(factorial Is Nothing)
    End Sub
End Module
        </file>
    </compilation>, OptionsExe)

            CompileAndVerify(comp1, <![CDATA[
Called lambda.
True
]]>)
        End Sub

        <WorkItem(540659)>
        <Fact>
        Public Sub LambdaForSub()
            Dim comp1 = CompilationUtils.CreateCompilationWithMscorlibAndVBRuntime(
    <compilation>
        <file name="a.vb">
Imports System
Module M1
    Function Bar(Of T)(ByVal x As T) As T
        Console.WriteLine("Hello Bug 7002")
    End Function
    Sub Main()
        Dim x As Func(Of Func(Of String, String)) = Function() AddressOf Bar
        x.Invoke().Invoke("Bug 7002")
    End Sub
End Module
        </file>
    </compilation>, OptionsExe)

            CompileAndVerify(comp1, <![CDATA[
Hello Bug 7002
]]>)
        End Sub

        <WorkItem(540658)>
        <Fact>
        Public Sub LambdaTest1()
            Dim comp1 = CompilationUtils.CreateCompilationWithMscorlibAndVBRuntime(
    <compilation>
        <file name="a.vb">
Imports System
Module M1
    Sub Foo(Of T)(ByVal x As T, ByVal y As T)
    End Sub
    Function Bar(Of T)(ByVal x As T) As T
    End Function
    Event ev(ByVal x As Integer, ByVal y As Integer)
    Sub Main()
        Dim x = New Func(Of String, String)(AddressOf Foo)
        Dim x1 = New Func(Of String, String)(AddressOf Bar)
        Dim x2 = Function() AddressOf Foo
        Dim x21 = Sub() Call Function() Sub() AddressOf Foo
        Dim x22 = Sub() Call Function() Sub() AddHandler ev, AddressOf Foo
        Dim x3 As Func(Of Func(Of String, String)) = Function() AddressOf Foo
        Dim x31 As Func(Of Func(Of String, String)) = Function() AddressOf Bar
    End Sub
End Module
        </file>
    </compilation>)
            AssertTheseDiagnostics(comp1,
<expected>
BC31143: Method 'Public Sub Foo(Of T)(x As T, y As T)' does not have a signature compatible with delegate 'Delegate Function System.Func(Of String, String)(arg As String) As String'.
        Dim x = New Func(Of String, String)(AddressOf Foo)
                                                      ~~~
BC30581: 'AddressOf' expression cannot be converted to 'Object' because 'Object' is not a delegate type.
        Dim x2 = Function() AddressOf Foo
                            ~~~~~~~~~~~~~
BC30035: Syntax error.
        Dim x21 = Sub() Call Function() Sub() AddressOf Foo
                                              ~~~~~~~~~
BC31143: Method 'Public Sub Foo(Of T)(x As T, y As T)' does not have a signature compatible with delegate 'Delegate Function System.Func(Of String, String)(arg As String) As String'.
        Dim x3 As Func(Of Func(Of String, String)) = Function() AddressOf Foo
                                                                          ~~~

</expected>)
        End Sub

        <WorkItem(528232)>
        <Fact()>
        Public Sub LambdaTest3()
            Dim comp1 = CompilationUtils.CreateCompilationWithMscorlibAndVBRuntime(
    <compilation>
        <file name="a.vb">
Imports System
Module M1
    Dim x As Func(Of System.Linq.Expressions.Expression(Of Action(Of Integer, Long))) = Function()
                                                                                Return Sub(x As Long, y As Long) Console.WriteLine(y = x)
                                                                            End Function
    Sub Main()
    End Sub
End Module
        </file>
    </compilation>)
            comp1 = comp1.AddReferences(LinqAssemblyRef)
            CompileAndVerify(comp1)
        End Sub

        <WorkItem(540660)>
        <Fact>
        Public Sub LambdaTest4()
            Dim comp1 = CompilationUtils.CreateCompilationWithMscorlibAndVBRuntime(
    <compilation>
        <file name="a.vb">
Imports System
Class C1
    Shared Sub Main()
    End Sub
    Dim i = Sub(a as Integer, b as Long)
    MustInherit Public Class C1
End Class
        </file>
    </compilation>)
            AssertTheseDiagnostics(comp1,
<expected>
BC30481: 'Class' statement must end with a matching 'End Class'.
Class C1
~~~~~~~~
BC36673: Multiline lambda expression is missing 'End Sub'.
    Dim i = Sub(a as Integer, b as Long)
            ~~~~~~~~~~~~~~~~~~~~~~~~~~~~
</expected>)
        End Sub

        <WorkItem(528233)>
        <Fact()>
        Public Sub LambdaTest5()
            Dim comp1 = CompilationUtils.CreateCompilationWithMscorlibAndVBRuntime(
    <compilation>
        <file name="a.vb">
Imports System
Module M1
    Sub Foo(ByVal x)
    End Sub
    Class C1
        Public Shared y As Action(Of Integer) = Sub(c) Call (Sub(b) M1.Foo(b))(c)
    End Class
    Sub Main()
    End Sub
End Module
        </file>
    </compilation>)
            CompileAndVerify(comp1)
        End Sub


        <WorkItem(541360)>
        <Fact>
        Public Sub ERR_30500_CircularEvaluation1()
            Dim comp1 = CompilationUtils.CreateCompilationWithMscorlibAndVBRuntime(
        <compilation>
            <file name="a.vb">
        Imports System

        Enum E
            A = 1 + A
        End Enum

        Module M
            Public Sub Main()
                Console.WriteLine(E.A)
            End Sub
        End Module
            </file>
        </compilation>)

            ' using VerifyEmitDiagnostics would not test this issue, because it calls GetAll...Errors internally.
            ' the error was about using emit directly, which resulted in a NRE.
            comp1.Emit(New MemoryStream(), "Test.exe", "Test.pdb", New MemoryStream()).Diagnostics.Verify(
                    Diagnostic(ERRID.ERR_CircularEvaluation1, "A").WithArguments("A")
                )
        End Sub

        <Fact(), WorkItem(543653)>
        Public Sub CompareToOnDecimalTypeChar()
            Dim comp = CompilationUtils.CreateCompilationWithMscorlibAndVBRuntime(
    <compilation>
        <file name="a.vb">
Module M1
    Const Curr1 = 5@
    Sub FOO()
        If Curr1 &lt;&gt; 5 Then
        End If
    End Sub
End Module
        </file>
    </compilation>)

            CompileAndVerify(comp)
        End Sub

#End Region

#Region "Mixed Error Tests"

        <Fact(), WorkItem(530211)>
        Public Sub ModuleNameMismatch()
            Dim netModule = CompilationUtils.CreateCompilationWithMscorlib(
    <compilation name="ModuleNameMismatch">
        <file name="a.vb">
Class Test
End Class
        </file>
    </compilation>, OptionsNetModule)

            CompileAndVerify(netModule, verify:=False)
            Dim moduleImage = netModule.EmitToArray()

            Dim tempDir = Temp.CreateDirectory()

            Dim match = tempDir.CreateFile("ModuleNameMismatch.netmodule")
            Dim mismatch = tempDir.CreateFile("ModuleNameMismatch.mod")
            match.WriteAllBytes(moduleImage)
            mismatch.WriteAllBytes(moduleImage)

            Dim source =
    <compilation>
        <file name="a.vb">
Module Module1
    Sub Main()
    End Sub
End Module
        </file>
    </compilation>

            Dim compilation1 = CompilationUtils.CreateCompilationWithMscorlibAndVBRuntimeAndReferences(source, {New MetadataFileReference(match.Path, MetadataImageKind.Module)}, OptionsExe)
            CompileAndVerify(compilation1)

            Dim compilation2 = CompilationUtils.CreateCompilationWithMscorlibAndVBRuntimeAndReferences(source, {New MetadataFileReference(mismatch.Path, MetadataImageKind.Module)}, OptionsExe)

            AssertTheseDiagnostics(compilation2,
<expected>
BC37205: Module name 'ModuleNameMismatch.netmodule' stored in 'ModuleNameMismatch.mod' must match its filename.
</expected>)

            Dim imageData = ModuleMetadata.CreateFromImage(moduleImage)

            Dim compilation3 = CompilationUtils.CreateCompilationWithMscorlibAndVBRuntimeAndReferences(source, {New MetadataImageReference(imageData, fullPath:=match.Path)}, OptionsExe)
            CompileAndVerify(compilation3)

            Dim compilation4 = CompilationUtils.CreateCompilationWithMscorlibAndVBRuntimeAndReferences(source, {New MetadataImageReference(imageData, fullPath:=mismatch.Path)}, OptionsExe)

            AssertTheseDiagnostics(compilation4,
<expected>
BC37205: Module name 'ModuleNameMismatch.netmodule' stored in 'ModuleNameMismatch.mod' must match its filename.
</expected>)
        End Sub

#End Region

    End Class

End Namespace

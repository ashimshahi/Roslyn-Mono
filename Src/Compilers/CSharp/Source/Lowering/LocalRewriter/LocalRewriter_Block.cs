﻿// Copyright (c) Microsoft Open Technologies, Inc.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
namespace Microsoft.CodeAnalysis.CSharp
{
    internal sealed partial class LocalRewriter
    {
        public override BoundNode VisitBlock(BoundBlock node)
        {
            if (node.WasCompilerGenerated || !this.generateDebugInfo)
            {
                return node.Update(node.LocalsOpt, VisitList(node.Statements));
            }

            BlockSyntax syntax = node.Syntax as BlockSyntax;
            Debug.Assert(syntax != null);

            var builder = ArrayBuilder<BoundStatement>.GetInstance();

            var oBspan = syntax.OpenBraceToken.Span;
            builder.Add(new BoundSequencePointWithSpan(syntax, null, oBspan));

            for (int i = 0; i < node.Statements.Length; i++)
            {
                var stmt = (BoundStatement)Visit(node.Statements[i]);
                if (stmt != null) builder.Add(stmt);
            }

            // no need to mark "}" on the outermost block
            // as it cannot leave it normally. The block will have "return" at the end.
            if (syntax.Parent == null || !(syntax.Parent.IsAnonymousFunction() || syntax.Parent is BaseMethodDeclarationSyntax))
            {
                var cBspan = syntax.CloseBraceToken.Span;
                builder.Add(new BoundSequencePointWithSpan(syntax, null, cBspan));
            }

            return new BoundBlock(syntax, node.LocalsOpt, builder.ToImmutableAndFree(), node.HasErrors);
        }

        public override BoundNode VisitNoOpStatement(BoundNoOpStatement node)
        {
            return (node.WasCompilerGenerated || !this.generateDebugInfo)
                ? new BoundBlock(node.Syntax, ImmutableArray<LocalSymbol>.Empty, ImmutableArray<BoundStatement>.Empty)
                : AddSequencePoint(node);
        }
    }
}

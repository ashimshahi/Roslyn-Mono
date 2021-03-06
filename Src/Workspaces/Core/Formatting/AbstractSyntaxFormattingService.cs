﻿// Copyright (c) Microsoft Open Technologies, Inc.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis.Formatting.Rules;
using Microsoft.CodeAnalysis.Options;
using Microsoft.CodeAnalysis.Shared;
using Microsoft.CodeAnalysis.Shared.Collections;
using Microsoft.CodeAnalysis.Shared.Utilities;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.Formatting
{
    internal abstract class AbstractSyntaxFormattingService : ISyntaxFormattingService
    {
        private static readonly Func<TextSpan, bool> NotEmpty = s => !s.IsEmpty;
        private static readonly Func<TextSpan, int> SpanLength = s => s.Length;

        protected AbstractSyntaxFormattingService()
        {
        }

        public abstract IEnumerable<IFormattingRule> GetDefaultFormattingRules();

        protected abstract IFormattingResult CreateAggregatedFormattingResult(SyntaxNode node, IList<AbstractFormattingResult> results, SimpleIntervalTree<TextSpan> formattingSpans = null);

        protected abstract AbstractFormattingResult Format(SyntaxNode node, OptionSet options, IEnumerable<IFormattingRule> rules, SyntaxToken token1, SyntaxToken token2, CancellationToken cancellationToken);

        public IFormattingResult Format(SyntaxNode node, IEnumerable<TextSpan> spans, OptionSet options, IEnumerable<IFormattingRule> rules, CancellationToken cancellationToken)
        {
            CheckArguments(node, spans, options, rules);

            // quick exit check
            var spansToFormat = new NormalizedTextSpanCollection(spans.Where(NotEmpty));
            if (spansToFormat.Count == 0)
            {
                return CreateAggregatedFormattingResult(node, SpecializedCollections.EmptyList<AbstractFormattingResult>());
            }

            // check what kind of formatting strategy to use
            if (ShouldUseCollapsedFormattingSpan(spansToFormat))
            {
                return FormatCollapsedSpan(node, options, rules, spansToFormat, cancellationToken);
            }

            return FormatIndividually(node, options, rules, spansToFormat, cancellationToken);
        }

        private IFormattingResult FormatCollapsedSpan(
            SyntaxNode node, OptionSet options, IEnumerable<IFormattingRule> rules, IList<TextSpan> spansToFormat, CancellationToken cancellationToken)
        {
            var spanToFormat = TextSpan.FromBounds(spansToFormat[0].Start, spansToFormat[spansToFormat.Count - 1].End);
            var pair = node.ConvertToTokenPair(spanToFormat);

            if (node.IsInvalidTokenRange(pair.Item1, pair.Item2))
            {
                return CreateAggregatedFormattingResult(node, SpecializedCollections.EmptyList<AbstractFormattingResult>());
            }

            // more expensive case
            var result = Format(node, options, rules, pair.Item1, pair.Item2, cancellationToken);
            return CreateAggregatedFormattingResult(node, new List<AbstractFormattingResult>(1) { result }, SimpleIntervalTree.Create(TextSpanIntervalIntrospector.Instance, spansToFormat));
        }

        private IFormattingResult FormatIndividually(
            SyntaxNode node, OptionSet options, IEnumerable<IFormattingRule> rules, IList<TextSpan> spansToFormat, CancellationToken cancellationToken)
        {
            List<AbstractFormattingResult> results = null;
            foreach (var pair in node.ConvertToTokenPairs(spansToFormat))
            {
                if (node.IsInvalidTokenRange(pair.Item1, pair.Item2))
                {
                    continue;
                }

                results = results ?? new List<AbstractFormattingResult>();
                results.Add(Format(node, options, rules, pair.Item1, pair.Item2, cancellationToken));
            }

            // quick simple case check
            if (results == null)
            {
                return CreateAggregatedFormattingResult(node, SpecializedCollections.EmptyList<AbstractFormattingResult>());
            }

            if (results.Count == 1)
            {
                return results[0];
            }

            // more expensive case
            return CreateAggregatedFormattingResult(node, results);
        }

        private bool ShouldUseCollapsedFormattingSpan(IList<TextSpan> list)
        {
            // most common case. it is either just formatting a whole file, a selection or some generate XXX refactoring.
            if (list.Count <= 3)
            {
                // don't collapse formatting spans
                return false;
            }

            // too many formatting spans, just collapse them and format at once
            if (list.Count > 30)
            {
                // figuring out base indentation at random place in a file takes about 2ms.
                // doing 30 times will make it cost about 60ms. that is about same cost, for the same file, engine will
                // take to create full formatting context. basically after that, creating full context is cheaper than
                // doing bottom up base indentation calculation for each span.
                return true;
            }

            // check how much area we are formatting
            var formattingSpan = TextSpan.FromBounds(list[0].Start, list[list.Count - 1].End);
            var actualFormattingSize = list.Sum(SpanLength);

            // we are formatting more than half of the collapsed span.
            return (formattingSpan.Length / Math.Max(actualFormattingSize, 1)) < 2;
        }

        private static void CheckArguments(SyntaxNode node, IEnumerable<TextSpan> spans, OptionSet options, IEnumerable<IFormattingRule> rules)
        {
            if (node == null)
            {
                throw new ArgumentNullException("node");
            }

            if (spans == null)
            {
                throw new ArgumentNullException("spans");
            }

            if (options == null)
            {
                throw new ArgumentNullException("options");
            }

            if (rules == null)
            {
                throw new ArgumentException("rules");
            }
        }
    }
}
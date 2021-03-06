﻿// Copyright (c) Microsoft Open Technologies, Inc.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Microsoft.CodeAnalysis
{
    public class ProjectDiagnostic : WorkspaceDiagnostic
    {
        public ProjectId ProjectId { get; private set; }

        public ProjectDiagnostic(WorkspaceDiagnosticKind kind, string message, ProjectId projectId)
            : base(kind, message)
        {
            this.ProjectId = projectId;
        }
    }
}
// Copyright (c) Richasy. All rights reserved.

using Microsoft.VisualStudio.TestTools.UnitTesting;

[assembly: Parallelize(Workers = 4, Scope = ExecutionScope.ClassLevel)]

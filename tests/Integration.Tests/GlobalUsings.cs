// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     GlobalUsings.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Integration.Tests
// =======================================================

global using System;
global using System.Diagnostics.CodeAnalysis;

global using Api.Data;
global using Api.Handlers.Categories;
global using Api.Handlers.Comments;
global using Api.Handlers.Issues;
global using Api.Handlers.Statuses;
global using Api.Services;

global using FluentAssertions;

global using FluentValidation;

global using MongoDB.Bson;

global using NSubstitute;

global using Shared.Abstractions;
global using Shared.DTOs;
global using Shared.Exceptions;
global using Shared.Validators;

global using Testcontainers.MongoDb;

global using Xunit;

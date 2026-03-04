// =======================================================
// Copyright (c) 2026. All rights reserved.
// File Name :     GlobalUsings.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Integration.Tests
// =======================================================

global using Xunit;
global using FluentAssertions;
global using FluentValidation;
global using Testcontainers.MongoDb;

global using Api.Data;
global using Api.Handlers.Categories;
global using Api.Handlers.Comments;
global using Api.Handlers.Statuses;
global using MongoDB.Bson;
global using Shared.DTOs;
global using System.Diagnostics.CodeAnalysis;
global using Api.Handlers.Issues;
global using Api.Services;
global using NSubstitute;
global using Shared.Abstractions;
global using Shared.Exceptions;
global using Shared.Validators;
global using System;

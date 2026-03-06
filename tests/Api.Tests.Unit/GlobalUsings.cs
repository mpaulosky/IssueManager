// ============================================
// Copyright (c) 2026. All rights reserved.
// File Name :     GlobalUsings.cs
// Company :       mpaulosky
// Author :        Matthew Paulosky
// Solution Name : IssueManager
// Project Name :  Api.Tests.Unit
// =============================================
global using System;
global using System.Diagnostics.CodeAnalysis;
global using System.Net;
global using System.Net.Http.Json;
global using System.Security.Claims;
global using System.Text.Encodings.Web;

global using Api.Data;
global using Api.Handlers.Categories;
global using Api.Handlers.Comments;
global using Api.Handlers.Issues;
global using Api.Handlers.Statuses;
global using Api.Services;

global using FluentAssertions;

global using FluentValidation;

global using Microsoft.AspNetCore.Authentication;
global using Microsoft.AspNetCore.Hosting;
global using Microsoft.AspNetCore.Http;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.Options;
global using Microsoft.Extensions.Primitives;

global using MongoDB.Bson;

global using NSubstitute;

global using Shared.Abstractions;
global using Shared.DTOs;
global using Shared.Validators;

global using Xunit;

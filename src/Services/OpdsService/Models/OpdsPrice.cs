// Copyright (c) Richasy. All rights reserved.

namespace Richasy.RodelReader.Services.OpdsService.Models;

/// <summary>
/// OPDS 价格信息.
/// </summary>
/// <param name="Value">价格数值.</param>
/// <param name="CurrencyCode">货币代码（如 USD, CNY）.</param>
public sealed record OpdsPrice(
    decimal Value,
    string CurrencyCode);

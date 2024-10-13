﻿using Microsoft.Graph;

namespace Generellem.DocumentSource;

/// <summary>
/// Instantiates MSGraph types.
/// </summary>
public interface IMSGraphClientFactory
{
    /// <summary>
    /// Instantiates a new <see cref="GraphServiceClient"/> for accessing MSGraph.
    /// </summary>
    /// <param name="scopes">The scopes to request.</param>
    /// <param name="baseUrl">The base URL to build a return URL for the OAuth flow.</param>
    /// <returns><see cref="GraphServiceClient"/></returns>
    Task<GraphServiceClient> CreateAsync(string scopes, string baseUrl);
}
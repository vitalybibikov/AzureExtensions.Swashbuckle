// Copyright (c) Vitaly Bibikov. All rights reserved.
// Licensed under the MIT License.

using System.Collections;
using System.Collections.Specialized;
using System.Net;
using System.Security.Claims;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace AzureFunctions.Extensions.Swashbuckle.Tests.TestHelpers;

internal sealed class FakeFunctionContext : FunctionContext
{
    public override string InvocationId { get; } = Guid.NewGuid().ToString();
    public override string FunctionId { get; } = "test-function";
    public override TraceContext TraceContext => null!;
    public override BindingContext BindingContext => null!;
    public override RetryContext RetryContext => null!;
    public override IServiceProvider InstanceServices { get; set; } = null!;
    public override FunctionDefinition FunctionDefinition => null!;
    public override IDictionary<object, object> Items { get; set; } = new Dictionary<object, object>();
    public override IInvocationFeatures Features { get; } = new FakeInvocationFeatures();
}

internal sealed class FakeInvocationFeatures : IInvocationFeatures
{
    private readonly Dictionary<Type, object> _features = new();

    public T? Get<T>() => _features.TryGetValue(typeof(T), out var value) ? (T)value : default;
    public void Set<T>(T instance) => _features[typeof(T)] = instance!;
    public IEnumerator<KeyValuePair<Type, object>> GetEnumerator() => _features.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

internal sealed class FakeHttpRequestData : HttpRequestData
{
    public FakeHttpRequestData(Uri url)
        : base(new FakeFunctionContext())
    {
        Url = url;
    }

    public override Stream Body { get; } = Stream.Null;
    public override HttpHeadersCollection Headers { get; } = new HttpHeadersCollection();
    public override IReadOnlyCollection<IHttpCookie> Cookies { get; } = Array.Empty<IHttpCookie>();
    public override Uri Url { get; }
    public override IEnumerable<ClaimsIdentity> Identities { get; } = Array.Empty<ClaimsIdentity>();
    public override string Method { get; } = "GET";

    public override HttpResponseData CreateResponse()
    {
        return new FakeHttpResponseData(FunctionContext);
    }
}

internal sealed class FakeHttpResponseData : HttpResponseData
{
    public FakeHttpResponseData(FunctionContext context) : base(context)
    {
    }

    public override HttpStatusCode StatusCode { get; set; }
    public override HttpHeadersCollection Headers { get; set; } = new HttpHeadersCollection();
    public override Stream Body { get; set; } = new MemoryStream();
    public override HttpCookies Cookies { get; } = new FakeHttpCookies();
}

internal sealed class FakeHttpCookies : HttpCookies
{
    public override void Append(string name, string value) { }
    public override void Append(IHttpCookie cookie) { }
    public override IHttpCookie CreateNew() => throw new NotImplementedException();
}

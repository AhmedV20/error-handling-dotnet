import{_ as i,o as a,c as n,a2 as l}from"./chunks/framework.-Q5tCgKN.js";const g=JSON.parse('{"title":"Configuration Template","description":"","frontmatter":{},"headers":[],"relativePath":"reference/template.md","filePath":"reference/template.md"}'),p={name:"reference/template.md"};function e(t,s,h,k,r,E){return a(),n("div",null,[...s[0]||(s[0]=[l(`<h1 id="configuration-template" tabindex="-1">Configuration Template <a class="header-anchor" href="#configuration-template" aria-label="Permalink to &quot;Configuration Template&quot;">​</a></h1><p>Full YAML configuration template with all available options and their defaults.</p><div class="language-yaml vp-adaptive-theme"><button title="Copy Code" class="copy"></button><span class="lang">yaml</span><pre class="shiki shiki-themes github-light github-dark vp-code" tabindex="0"><code><span class="line"><span style="--shiki-light:#6A737D;--shiki-dark:#6A737D;"># ErrorLens.ErrorHandling Configuration Template</span></span>
<span class="line"><span style="--shiki-light:#6A737D;--shiki-dark:#6A737D;"># Copy this file to your project as &#39;errorhandling.yml&#39;</span></span>
<span class="line"><span style="--shiki-light:#6A737D;--shiki-dark:#6A737D;"># and configure as needed.</span></span>
<span class="line"></span>
<span class="line"><span style="--shiki-light:#22863A;--shiki-dark:#85E89D;">ErrorHandling</span><span style="--shiki-light:#24292E;--shiki-dark:#E1E4E8;">:</span></span>
<span class="line"><span style="--shiki-light:#6A737D;--shiki-dark:#6A737D;">  # Enable or disable error handling globally</span></span>
<span class="line"><span style="--shiki-light:#22863A;--shiki-dark:#85E89D;">  Enabled</span><span style="--shiki-light:#24292E;--shiki-dark:#E1E4E8;">: </span><span style="--shiki-light:#005CC5;--shiki-dark:#79B8FF;">true</span></span>
<span class="line"></span>
<span class="line"><span style="--shiki-light:#6A737D;--shiki-dark:#6A737D;">  # Include HTTP status code in JSON response body</span></span>
<span class="line"><span style="--shiki-light:#22863A;--shiki-dark:#85E89D;">  HttpStatusInJsonResponse</span><span style="--shiki-light:#24292E;--shiki-dark:#E1E4E8;">: </span><span style="--shiki-light:#005CC5;--shiki-dark:#79B8FF;">false</span></span>
<span class="line"></span>
<span class="line"><span style="--shiki-light:#6A737D;--shiki-dark:#6A737D;">  # Error code generation strategy: AllCaps or FullQualifiedName</span></span>
<span class="line"><span style="--shiki-light:#22863A;--shiki-dark:#85E89D;">  DefaultErrorCodeStrategy</span><span style="--shiki-light:#24292E;--shiki-dark:#E1E4E8;">: </span><span style="--shiki-light:#032F62;--shiki-dark:#9ECBFF;">AllCaps</span></span>
<span class="line"></span>
<span class="line"><span style="--shiki-light:#6A737D;--shiki-dark:#6A737D;">  # Search exception base class hierarchy for configuration matches</span></span>
<span class="line"><span style="--shiki-light:#22863A;--shiki-dark:#85E89D;">  SearchSuperClassHierarchy</span><span style="--shiki-light:#24292E;--shiki-dark:#E1E4E8;">: </span><span style="--shiki-light:#005CC5;--shiki-dark:#79B8FF;">false</span></span>
<span class="line"></span>
<span class="line"><span style="--shiki-light:#6A737D;--shiki-dark:#6A737D;">  # Include property path in field error responses</span></span>
<span class="line"><span style="--shiki-light:#22863A;--shiki-dark:#85E89D;">  AddPathToError</span><span style="--shiki-light:#24292E;--shiki-dark:#E1E4E8;">: </span><span style="--shiki-light:#005CC5;--shiki-dark:#79B8FF;">true</span></span>
<span class="line"></span>
<span class="line"><span style="--shiki-light:#6A737D;--shiki-dark:#6A737D;">  # Intercept [ApiController] model validation to use ErrorLens format</span></span>
<span class="line"><span style="--shiki-light:#22863A;--shiki-dark:#85E89D;">  OverrideModelStateValidation</span><span style="--shiki-light:#24292E;--shiki-dark:#E1E4E8;">: </span><span style="--shiki-light:#005CC5;--shiki-dark:#79B8FF;">false</span></span>
<span class="line"></span>
<span class="line"><span style="--shiki-light:#6A737D;--shiki-dark:#6A737D;">  # Use RFC 9457 Problem Details format</span></span>
<span class="line"><span style="--shiki-light:#22863A;--shiki-dark:#85E89D;">  UseProblemDetailFormat</span><span style="--shiki-light:#24292E;--shiki-dark:#E1E4E8;">: </span><span style="--shiki-light:#005CC5;--shiki-dark:#79B8FF;">false</span></span>
<span class="line"></span>
<span class="line"><span style="--shiki-light:#6A737D;--shiki-dark:#6A737D;">  # URI prefix for Problem Details &#39;type&#39; field</span></span>
<span class="line"><span style="--shiki-light:#22863A;--shiki-dark:#85E89D;">  ProblemDetailTypePrefix</span><span style="--shiki-light:#24292E;--shiki-dark:#E1E4E8;">: </span><span style="--shiki-light:#032F62;--shiki-dark:#9ECBFF;">&quot;https://example.com/errors/&quot;</span></span>
<span class="line"></span>
<span class="line"><span style="--shiki-light:#6A737D;--shiki-dark:#6A737D;">  # Convert error codes to kebab-case in Problem Details &#39;type&#39; field</span></span>
<span class="line"><span style="--shiki-light:#22863A;--shiki-dark:#85E89D;">  ProblemDetailConvertToKebabCase</span><span style="--shiki-light:#24292E;--shiki-dark:#E1E4E8;">: </span><span style="--shiki-light:#005CC5;--shiki-dark:#79B8FF;">true</span></span>
<span class="line"></span>
<span class="line"><span style="--shiki-light:#6A737D;--shiki-dark:#6A737D;">  # Exception logging verbosity: None, MessageOnly, WithStacktrace</span></span>
<span class="line"><span style="--shiki-light:#22863A;--shiki-dark:#85E89D;">  ExceptionLogging</span><span style="--shiki-light:#24292E;--shiki-dark:#E1E4E8;">: </span><span style="--shiki-light:#032F62;--shiki-dark:#9ECBFF;">MessageOnly</span></span>
<span class="line"></span>
<span class="line"><span style="--shiki-light:#6A737D;--shiki-dark:#6A737D;">  # Custom JSON field names for error responses</span></span>
<span class="line"><span style="--shiki-light:#22863A;--shiki-dark:#85E89D;">  JsonFieldNames</span><span style="--shiki-light:#24292E;--shiki-dark:#E1E4E8;">:</span></span>
<span class="line"><span style="--shiki-light:#22863A;--shiki-dark:#85E89D;">    Code</span><span style="--shiki-light:#24292E;--shiki-dark:#E1E4E8;">: </span><span style="--shiki-light:#032F62;--shiki-dark:#9ECBFF;">code</span></span>
<span class="line"><span style="--shiki-light:#22863A;--shiki-dark:#85E89D;">    Message</span><span style="--shiki-light:#24292E;--shiki-dark:#E1E4E8;">: </span><span style="--shiki-light:#032F62;--shiki-dark:#9ECBFF;">message</span></span>
<span class="line"><span style="--shiki-light:#22863A;--shiki-dark:#85E89D;">    Status</span><span style="--shiki-light:#24292E;--shiki-dark:#E1E4E8;">: </span><span style="--shiki-light:#032F62;--shiki-dark:#9ECBFF;">status</span></span>
<span class="line"><span style="--shiki-light:#22863A;--shiki-dark:#85E89D;">    FieldErrors</span><span style="--shiki-light:#24292E;--shiki-dark:#E1E4E8;">: </span><span style="--shiki-light:#032F62;--shiki-dark:#9ECBFF;">fieldErrors</span></span>
<span class="line"><span style="--shiki-light:#22863A;--shiki-dark:#85E89D;">    GlobalErrors</span><span style="--shiki-light:#24292E;--shiki-dark:#E1E4E8;">: </span><span style="--shiki-light:#032F62;--shiki-dark:#9ECBFF;">globalErrors</span></span>
<span class="line"><span style="--shiki-light:#22863A;--shiki-dark:#85E89D;">    ParameterErrors</span><span style="--shiki-light:#24292E;--shiki-dark:#E1E4E8;">: </span><span style="--shiki-light:#032F62;--shiki-dark:#9ECBFF;">parameterErrors</span></span>
<span class="line"><span style="--shiki-light:#22863A;--shiki-dark:#85E89D;">    Property</span><span style="--shiki-light:#24292E;--shiki-dark:#E1E4E8;">: </span><span style="--shiki-light:#032F62;--shiki-dark:#9ECBFF;">property</span></span>
<span class="line"><span style="--shiki-light:#22863A;--shiki-dark:#85E89D;">    RejectedValue</span><span style="--shiki-light:#24292E;--shiki-dark:#E1E4E8;">: </span><span style="--shiki-light:#032F62;--shiki-dark:#9ECBFF;">rejectedValue</span></span>
<span class="line"><span style="--shiki-light:#22863A;--shiki-dark:#85E89D;">    Path</span><span style="--shiki-light:#24292E;--shiki-dark:#E1E4E8;">: </span><span style="--shiki-light:#032F62;--shiki-dark:#9ECBFF;">path</span></span>
<span class="line"><span style="--shiki-light:#22863A;--shiki-dark:#85E89D;">    Parameter</span><span style="--shiki-light:#24292E;--shiki-dark:#E1E4E8;">: </span><span style="--shiki-light:#032F62;--shiki-dark:#9ECBFF;">parameter</span></span>
<span class="line"></span>
<span class="line"><span style="--shiki-light:#6A737D;--shiki-dark:#6A737D;">  # Exception type → HTTP status code mappings</span></span>
<span class="line"><span style="--shiki-light:#22863A;--shiki-dark:#85E89D;">  HttpStatuses</span><span style="--shiki-light:#24292E;--shiki-dark:#E1E4E8;">:</span></span>
<span class="line"><span style="--shiki-light:#6A737D;--shiki-dark:#6A737D;">    # MyApp.Exceptions.UserNotFoundException: 404</span></span>
<span class="line"><span style="--shiki-light:#6A737D;--shiki-dark:#6A737D;">    # MyApp.Exceptions.DuplicateEmailException: 409</span></span>
<span class="line"><span style="--shiki-light:#6A737D;--shiki-dark:#6A737D;">    # MyApp.Exceptions.ForbiddenException: 403</span></span>
<span class="line"></span>
<span class="line"><span style="--shiki-light:#6A737D;--shiki-dark:#6A737D;">  # Exception type or field.validation → error code mappings</span></span>
<span class="line"><span style="--shiki-light:#22863A;--shiki-dark:#85E89D;">  Codes</span><span style="--shiki-light:#24292E;--shiki-dark:#E1E4E8;">:</span></span>
<span class="line"><span style="--shiki-light:#6A737D;--shiki-dark:#6A737D;">    # MyApp.Exceptions.UserNotFoundException: USER_NOT_FOUND</span></span>
<span class="line"><span style="--shiki-light:#6A737D;--shiki-dark:#6A737D;">    # email.Required: EMAIL_IS_REQUIRED</span></span>
<span class="line"><span style="--shiki-light:#6A737D;--shiki-dark:#6A737D;">    # email.EmailAddress: EMAIL_FORMAT_INVALID</span></span>
<span class="line"></span>
<span class="line"><span style="--shiki-light:#6A737D;--shiki-dark:#6A737D;">  # Exception type or field.validation → error message mappings</span></span>
<span class="line"><span style="--shiki-light:#22863A;--shiki-dark:#85E89D;">  Messages</span><span style="--shiki-light:#24292E;--shiki-dark:#E1E4E8;">:</span></span>
<span class="line"><span style="--shiki-light:#6A737D;--shiki-dark:#6A737D;">    # MyApp.Exceptions.UserNotFoundException: The requested user was not found</span></span>
<span class="line"><span style="--shiki-light:#6A737D;--shiki-dark:#6A737D;">    # email.Required: A valid email address is required</span></span>
<span class="line"></span>
<span class="line"><span style="--shiki-light:#6A737D;--shiki-dark:#6A737D;">  # HTTP status code or pattern → log level mappings</span></span>
<span class="line"><span style="--shiki-light:#22863A;--shiki-dark:#85E89D;">  LogLevels</span><span style="--shiki-light:#24292E;--shiki-dark:#E1E4E8;">:</span></span>
<span class="line"><span style="--shiki-light:#6A737D;--shiki-dark:#6A737D;">    # 4xx: Warning</span></span>
<span class="line"><span style="--shiki-light:#6A737D;--shiki-dark:#6A737D;">    # 5xx: Error</span></span>
<span class="line"><span style="--shiki-light:#6A737D;--shiki-dark:#6A737D;">    # 404: Debug</span></span>
<span class="line"></span>
<span class="line"><span style="--shiki-light:#6A737D;--shiki-dark:#6A737D;">  # HTTP status codes/patterns that force full stack trace logging</span></span>
<span class="line"><span style="--shiki-light:#22863A;--shiki-dark:#85E89D;">  FullStacktraceHttpStatuses</span><span style="--shiki-light:#24292E;--shiki-dark:#E1E4E8;">:</span></span>
<span class="line"><span style="--shiki-light:#6A737D;--shiki-dark:#6A737D;">    # - 5xx</span></span>
<span class="line"><span style="--shiki-light:#6A737D;--shiki-dark:#6A737D;">    # - 400</span></span>
<span class="line"></span>
<span class="line"><span style="--shiki-light:#6A737D;--shiki-dark:#6A737D;">  # Exception types that force full stack trace logging</span></span>
<span class="line"><span style="--shiki-light:#22863A;--shiki-dark:#85E89D;">  FullStacktraceClasses</span><span style="--shiki-light:#24292E;--shiki-dark:#E1E4E8;">:</span></span>
<span class="line"><span style="--shiki-light:#6A737D;--shiki-dark:#6A737D;">    # - System.NullReferenceException</span></span>
<span class="line"><span style="--shiki-light:#6A737D;--shiki-dark:#6A737D;">    # - MyApp.Exceptions.CriticalException</span></span>
<span class="line"></span>
<span class="line"><span style="--shiki-light:#6A737D;--shiki-dark:#6A737D;">  # OpenAPI schema generation settings</span></span>
<span class="line"><span style="--shiki-light:#22863A;--shiki-dark:#85E89D;">  OpenApi</span><span style="--shiki-light:#24292E;--shiki-dark:#E1E4E8;">:</span></span>
<span class="line"><span style="--shiki-light:#22863A;--shiki-dark:#85E89D;">    DefaultStatusCodes</span><span style="--shiki-light:#24292E;--shiki-dark:#E1E4E8;">:</span></span>
<span class="line"><span style="--shiki-light:#24292E;--shiki-dark:#E1E4E8;">      - </span><span style="--shiki-light:#005CC5;--shiki-dark:#79B8FF;">400</span></span>
<span class="line"><span style="--shiki-light:#24292E;--shiki-dark:#E1E4E8;">      - </span><span style="--shiki-light:#005CC5;--shiki-dark:#79B8FF;">404</span></span>
<span class="line"><span style="--shiki-light:#24292E;--shiki-dark:#E1E4E8;">      - </span><span style="--shiki-light:#005CC5;--shiki-dark:#79B8FF;">500</span></span>
<span class="line"></span>
<span class="line"><span style="--shiki-light:#6A737D;--shiki-dark:#6A737D;">  # Rate limiting response settings (.NET 7+)</span></span>
<span class="line"><span style="--shiki-light:#22863A;--shiki-dark:#85E89D;">  RateLimiting</span><span style="--shiki-light:#24292E;--shiki-dark:#E1E4E8;">:</span></span>
<span class="line"><span style="--shiki-light:#22863A;--shiki-dark:#85E89D;">    ErrorCode</span><span style="--shiki-light:#24292E;--shiki-dark:#E1E4E8;">: </span><span style="--shiki-light:#032F62;--shiki-dark:#9ECBFF;">RATE_LIMIT_EXCEEDED</span></span>
<span class="line"><span style="--shiki-light:#22863A;--shiki-dark:#85E89D;">    DefaultMessage</span><span style="--shiki-light:#24292E;--shiki-dark:#E1E4E8;">: </span><span style="--shiki-light:#032F62;--shiki-dark:#9ECBFF;">&quot;Too many requests. Please try again later.&quot;</span></span>
<span class="line"><span style="--shiki-light:#22863A;--shiki-dark:#85E89D;">    IncludeRetryAfterInBody</span><span style="--shiki-light:#24292E;--shiki-dark:#E1E4E8;">: </span><span style="--shiki-light:#005CC5;--shiki-dark:#79B8FF;">true</span></span>
<span class="line"><span style="--shiki-light:#22863A;--shiki-dark:#85E89D;">    UseModernHeaderFormat</span><span style="--shiki-light:#24292E;--shiki-dark:#E1E4E8;">: </span><span style="--shiki-light:#005CC5;--shiki-dark:#79B8FF;">false</span></span></code></pre></div>`,3)])])}const o=i(p,[["render",e]]);export{g as __pageData,o as default};

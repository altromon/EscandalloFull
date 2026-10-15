# Third-Party Software Notices and Information

This project, **Escandallo Estética**, incorporates open-source software packages under the terms of their respective licenses.

---

## 1. Production Runtime (`src/`)

The production application codebase (`src/Escandallo.Core`, `src/Escandallo.Api`, `src/Escandallo.Web`) utilizes **0 third-party packages**. It operates exclusively upon Microsoft .NET 10 Web SDK framework libraries, with handwritten vanilla HTML, CSS, and JavaScript.

---

## 2. Test Infrastructure and Quality Tooling (`tests/`)

The automated testing harness and quality gates utilize the following open-source dependencies:

| Package | Version | License | Copyright / Author | URL |
| :--- | :--- | :---: | :--- | :--- |
| **coverlet.collector** | 6.0.4 | MIT | Toni Solarin-Sodara | [NuGet](https://www.nuget.org/packages/coverlet.collector/6.0.4) |
| **Microsoft.Bcl.AsyncInterfaces** | 6.0.0 | MIT | Microsoft Corporation | [NuGet](https://www.nuget.org/packages/Microsoft.Bcl.AsyncInterfaces/6.0.0) |
| **Microsoft.CodeCoverage** | 17.14.1 | MIT | Microsoft Corporation | [NuGet](https://www.nuget.org/packages/Microsoft.CodeCoverage/17.14.1) |
| **Microsoft.NET.Test.Sdk** | 17.14.1 | MIT | Microsoft Corporation | [NuGet](https://www.nuget.org/packages/Microsoft.NET.Test.Sdk/17.14.1) |
| **Microsoft.Playwright** | 1.63.0 | MIT | Microsoft Corporation | [NuGet](https://www.nuget.org/packages/Microsoft.Playwright/1.63.0) |
| **Microsoft.TestPlatform.ObjectModel** | 17.14.1 | MIT | Microsoft Corporation | [NuGet](https://www.nuget.org/packages/Microsoft.TestPlatform.ObjectModel/17.14.1) |
| **Microsoft.TestPlatform.TestHost** | 17.14.1 | MIT | Microsoft Corporation | [NuGet](https://www.nuget.org/packages/Microsoft.TestPlatform.TestHost/17.14.1) |
| **Newtonsoft.Json** | 13.0.3 | MIT | James Newton-King | [NuGet](https://www.nuget.org/packages/Newtonsoft.Json/13.0.3) |
| **System.ComponentModel.Annotations** | 5.0.0 | MIT | Microsoft Corporation | [NuGet](https://www.nuget.org/packages/System.ComponentModel.Annotations/5.0.0) |
| **xunit** | 2.9.3 | Apache-2.0 | James Newkirk, Brad Wilson | [NuGet](https://www.nuget.org/packages/xunit/2.9.3) |
| **xunit.abstractions** | 2.0.3 | Apache-2.0 | James Newkirk, Brad Wilson | [NuGet](https://www.nuget.org/packages/xunit.abstractions/2.0.3) |
| **xunit.analyzers** | 1.18.0 | Apache-2.0 | James Newkirk, Brad Wilson | [NuGet](https://www.nuget.org/packages/xunit.analyzers/1.18.0) |
| **xunit.assert** | 2.9.3 | Apache-2.0 | James Newkirk, Brad Wilson | [NuGet](https://www.nuget.org/packages/xunit.assert/2.9.3) |
| **xunit.core** | 2.9.3 | Apache-2.0 | James Newkirk, Brad Wilson | [NuGet](https://www.nuget.org/packages/xunit.core/2.9.3) |
| **xunit.extensibility.core** | 2.9.3 | Apache-2.0 | James Newkirk, Brad Wilson | [NuGet](https://www.nuget.org/packages/xunit.extensibility.core/2.9.3) |
| **xunit.extensibility.execution** | 2.9.3 | Apache-2.0 | James Newkirk, Brad Wilson | [NuGet](https://www.nuget.org/packages/xunit.extensibility.execution/2.9.3) |
| **xunit.runner.visualstudio** | 3.1.4 | Apache-2.0 | James Newkirk, Brad Wilson | [NuGet](https://www.nuget.org/packages/xunit.runner.visualstudio/3.1.4) |

---

## 3. License Texts

### MIT License

```text
The MIT License (MIT)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
```

### Apache License Version 2.0

```text
                                 Apache License
                           Version 2.0, January 2004
                        http://www.apache.org/licenses/

   TERMS AND CONDITIONS FOR USE, REPRODUCTION, AND DISTRIBUTION

   1. Definitions.
      "License" shall mean the terms and conditions for use, reproduction,
      and distribution as defined by Sections 1 through 9 of this document.
   ...
   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
```

/*************************************************************************
* Rainier Card Definition Fetcher
* (c) 2022 Hastwell/Electrosheep Networks 
* 
* This program is free software: you can redistribute it and/or modify
* it under the terms of the GNU Affero General Public License as published
* by the Free Software Foundation, either version 3 of the License, or
* (at your option) any later version.
* 
* This program is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
* GNU Affero General Public License for more details.
* 
* You should have received a copy of the GNU Affero General Public License
* along with this program.  If not, see <http://www.gnu.org/licenses/>.
**************************************************************************/

using ClientNetworking.Models;
using ClientNetworking.Models.Config;
using ClientNetworking;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Omukade.Tools.RainierCardDefinitionFetcher
{
    internal static class ClientExtensions
    {
        static internal bool DIE_ON_ERROR = true;

        static void GenericErrorHandler(IClient sdk, HttpResponseMessage response, ErrorResponse error)
        {
            if (error.networkException != null) AnsiConsole.WriteException(error.networkException);
            else if (error.errors?.Any() == true)
            {
                string errorString = string.Join(" ::: ", error.errors.Select(e => $"{e.code} - {e.message}"));
                AnsiConsole.WriteException(new Exception("Service error(s) - " + errorString));
            }
            else
            {
                AnsiConsole.WriteException(new Exception("Unknown service error"));
            }

            if(DIE_ON_ERROR)
            {
                Environment.Exit(1);
            }
        }

        public static TResult MakeSyncCall<TResult>(this Client client, Func<ResponseHandler<TResult>, ErrorHandler, Task> queryCall)
        {
            TResult queryResult = default;
            queryCall.Invoke((client, result) => queryResult = result, GenericErrorHandler).Wait();
            return queryResult;
        }

        public static TResult MakeSyncCall<TParam1, TResult>(this Client client, Func<TParam1, ResponseHandler<TResult>, ErrorHandler, Task> queryCall, TParam1 param1)
        {
            TResult queryResult = default;
            queryCall.Invoke(param1, (client, result) => queryResult = result, GenericErrorHandler).Wait();
            return queryResult;
        }

        public static TResult MakeSyncCall<TParam1, TParam2, TResult>(this Client client, Func<TParam1, TParam2, ResponseHandler<TResult>, ErrorHandler, Task> queryCall, TParam1 param1, TParam2 param2)
        {
            TResult queryResult = default;
            queryCall.Invoke(param1, param2, (client, result) => queryResult = result, GenericErrorHandler).Wait();
            return queryResult;
        }

        public static TResult MakeSyncCall<TParam1, TParam2, TParam3, TResult>(this Client client, Func<TParam1, TParam2, TParam3, ResponseHandler<TResult>, ErrorHandler, Task> queryCall, TParam1 param1, TParam2 param2, TParam3 param3)
        {
            TResult queryResult = default;
            queryCall.Invoke(param1, param2, param3, (client, result) => queryResult = result, GenericErrorHandler).Wait();
            return queryResult;
        }

        public static ConfigDocumentGetResponse GetConfigDocumentSync(this Client client, string configDocumentName)
            => client.MakeSyncCall<string, string, ConfigDocumentGetResponse>(client.GetConfigDocumentAsync, configDocumentName, /*ifNewerThanVersion*/ null);

        public static async Task<TResult> MakeAsyncCall<TResult>(this Client client, Func<ResponseHandler<TResult>, ErrorHandler, Task> queryCall)
        {
            TResult queryResult = default;
            await queryCall.Invoke((client, result) => queryResult = result, GenericErrorHandler);
            return queryResult;
        }

        public static async Task<TResult> MakeAsyncCall<TParam1, TResult>(this Client client, Func<TParam1, ResponseHandler<TResult>, ErrorHandler, Task> queryCall, TParam1 param1)
        {
            TResult queryResult = default;
            await queryCall.Invoke(param1, (client, result) => queryResult = result, GenericErrorHandler);
            return queryResult;
        }

        public static async Task<TResult> MakeAsyncCall<TParam1, TParam2, TResult>(this Client client, Func<TParam1, TParam2, ResponseHandler<TResult>, ErrorHandler, Task> queryCall, TParam1 param1, TParam2 param2)
        {
            TResult queryResult = default;
            await queryCall.Invoke(param1, param2, (client, result) => queryResult = result, GenericErrorHandler);
            return queryResult;
        }

        public static async Task<TResult> MakeAsyncCall<TParam1, TParam2, TParam3, TResult>(this Client client, Func<TParam1, TParam2, TParam3, ResponseHandler<TResult>, ErrorHandler, Task> queryCall, TParam1 param1, TParam2 param2, TParam3 param3)
        {
            TResult queryResult = default;
            await queryCall.Invoke(param1, param2, param3, (client, result) => queryResult = result, GenericErrorHandler);
            return queryResult;
        }
    }
}

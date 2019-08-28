/*
 *   _____                                ______
 *  /_   /  ____  ____  ____  _________  / __/ /_
 *    / /  / __ \/ __ \/ __ \/ ___/ __ \/ /_/ __/
 *   / /__/ /_/ / / / / /_/ /\_ \/ /_/ / __/ /_
 *  /____/\____/_/ /_/\__  /____/\____/_/  \__/
 *                   /____/
 *
 * Authors:
 *   钟峰(Popeye Zhong) <zongsoft@qq.com>
 *
 * Copyright (C) 2018-2019 Zongsoft Corporation <http://www.zongsoft.com>
 *
 * This file is part of Zongsoft.Externals.Wechat.
 *
 * Zongsoft.Externals.Wechat is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 2.1 of the License, or (at your option) any later version.
 *
 * Zongsoft.Externals.Wechat is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
 * Lesser General Public License for more details.
 *
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with Zongsoft.Externals.Wechat; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA
 */

using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Zongsoft.Externals.Wechat
{
	internal static class Utility
	{
		public static bool TryGetJson<T>(this HttpResponseMessage response, out T data)
		{
			data = default(T);

			if(response == null)
				return false;

			if(string.Equals(response.Content.Headers.ContentType.MediaType, "application/json", StringComparison.OrdinalIgnoreCase) ||
			   string.Equals(response.Content.Headers.ContentType.MediaType, "text/json", StringComparison.OrdinalIgnoreCase))
			{
				var content = response.Content.ReadAsStringAsync()
				                      .ConfigureAwait(false)
				                      .GetAwaiter()
				                      .GetResult();

				if(content != null && content.Length > 0)
				{
					data = Zongsoft.Runtime.Serialization.Serializer.Json.Deserialize<T>(content);
					return true;
				}
			}

			return false;
		}
	}
}

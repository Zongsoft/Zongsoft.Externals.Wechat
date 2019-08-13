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
using System.Web.Http;

namespace Zongsoft.Externals.Wechat.Controllers
{
	public class FallbackController : ApiController
	{
		public object Get(string signature, uint timestamp, string nonce)
		{
			if(Zongsoft.Common.UriExtension.TryGetQueryString(this.Request.RequestUri, "echostr", out var result))
				return this.Text(result);

			return new System.Web.Http.Results.StatusCodeResult(System.Net.HttpStatusCode.NoContent, this);
		}

		private void PrintRequestInfo()
		{
			var text = new System.Text.StringBuilder();

			text.Append("(" + this.Request.Method.Method + ")");
			text.AppendLine(this.Request.RequestUri.ToString());

			foreach(var header in this.Request.Headers)
			{
				text.AppendLine(header.Key + ":" + string.Join(";", header.Value));
			}

			Zongsoft.Diagnostics.Logger.Error(text.ToString());
		}

		protected System.Net.Http.HttpResponseMessage Text(string text, System.Text.Encoding encoding = null)
		{
			return new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.OK)
			{
				Content = new System.Net.Http.StringContent(text, encoding ?? System.Text.Encoding.UTF8, "text/plain")
			};
		}
	}
}

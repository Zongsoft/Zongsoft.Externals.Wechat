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
using System.Threading.Tasks;

namespace Zongsoft.Externals.Wechat
{
	/// <summary>
	/// 提供微信后台服务各种凭证的接口。
	/// </summary>
	public interface ICredentialProvider
	{
		/// <summary>
		/// 获取微信后台服务的访问标记(AccessToken)。
		/// </summary>
		/// <param name="appId">指定的微信应用编号。</param>
		/// <returns>如果获取成功则返回对应的访问标记，否则返回空(null)。</returns>
		Task<string> GetCredentialAsync(string appId);

		/// <summary>
		/// 获取微信后台服务的前端票据(JS_Ticket)。
		/// </summary>
		/// <param name="appId">指定的微信应用编号。</param>
		/// <returns>如果获取成功则返回对应的前端票据，否则返回空(null)。</returns>
		Task<string> GetTicketAsync(string appId);
	}
}

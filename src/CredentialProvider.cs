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
using System.Net.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

using Zongsoft.Services;
using Zongsoft.Runtime.Caching;

namespace Zongsoft.Externals.Wechat
{
	public class CredentialProvider : ICredentialProvider
	{
		#region 成员字段
		private HttpClient _http;
		private readonly Dictionary<string, Token> _localCache;
		#endregion

		#region 构造函数
		public CredentialProvider()
		{
			_http = new HttpClient();
			_localCache = new Dictionary<string, Token>();
		}
		#endregion

		#region 公共属性
		[ServiceDependency]
		public ICache Cache
		{
			get; set;
		}

		public Options.IConfiguration Configuration
		{
			get; set;
		}
		#endregion

		#region 公共方法
		public Task<string> GetCredentialAsync(string appId)
		{
			return this.GetCredentialAsync(GetApp(appId));
		}

		public async Task<string> GetCredentialAsync(Options.IAppSetting app)
		{
			if(app == null)
				throw new ArgumentNullException(nameof(app));

			var key = GetCredentalKey(app.Id);

			//首先从本地内存缓存中获取凭证标记，如果获取成功并且凭证未过期则返回该凭证号
			if(_localCache.TryGetValue(key, out var token) && !token.IsExpired)
				return token.Key;

			var credentialId = this.Cache.GetValue<string>(key);

			if(string.IsNullOrEmpty(credentialId))
			{
				token = await this.GetRemoteCredentialAsync(app.Id, app.Secret);

				if(this.Cache.SetValue(key, token.Key, token.Expiry))
					_localCache[key] = token;

				return token.Key;
			}
			else
			{
				var expiry = this.Cache.GetExpiry(key);

				if(expiry.HasValue)
					_localCache[key] = new CredentialToken(credentialId, DateTime.UtcNow.Add(expiry.Value));

				return credentialId;
			}
		}

		public Task<string> GetTicketAsync(string appId)
		{
			return this.GetTicketAsync(GetApp(appId));
		}

		public async Task<string> GetTicketAsync(Options.IAppSetting app)
		{
			if(app == null)
				throw new ArgumentNullException(nameof(app));

			var key = GetTicketKey(app.Id);

			//首先从本地内存缓存中获取票据标记，如果获取成功并且票据未过期则返回该票据号
			if(_localCache.TryGetValue(key, out var token) && !token.IsExpired)
				return token.Key;

			var ticketId = this.Cache.GetValue<string>(key);

			if(string.IsNullOrEmpty(ticketId))
			{
				token = await this.GetRemoteTicketAsync(await this.GetCredentialAsync(app));

				if(this.Cache.SetValue(key, token.Key, token.Expiry))
					_localCache[key] = token;

				return token.Key;
			}
			else
			{
				var expiry = this.Cache.GetExpiry(key);

				if(expiry.HasValue)
					_localCache[key] = new TicketToken(ticketId, DateTime.UtcNow.Add(expiry.Value));

				return ticketId;
			}
		}
		#endregion

		#region 私有方法
		private async Task<Token> GetRemoteCredentialAsync(string appId, string secret, int retries = 3)
		{
			var response = await _http.GetAsync($"https://api.weixin.qq.com/cgi-bin/token?grant_type=client_credential&appid={appId}&secret={secret}");

			if(response.IsSuccessStatusCode && response.TryGetJson<CredentialToken>(out var token) && token.IsValid)
				return token;

			if(response.TryGetJson<ErrorMessage>(out var error) && error.Code != 0)
			{
				if(error.Code == ErrorCodes.Busy && retries > 0)
				{
					await Task.Delay(Math.Max(500, Zongsoft.Common.Randomizer.GenerateInt32() % 2500));
					return await this.GetRemoteCredentialAsync(appId, secret, retries - 1);
				}

				throw new WechatException(error.Code, error.Message);
			}

			throw new WechatException(await response.Content.ReadAsStringAsync());
		}

		private async Task<Token> GetRemoteTicketAsync(string credentialId, int retries = 3)
		{
			var response = await _http.GetAsync($"https://api.weixin.qq.com/cgi-bin/ticket/getticket?access_token={credentialId}&type=jsapi");

			if(response.IsSuccessStatusCode && response.TryGetJson<TicketToken>(out var token) && token.IsValid)
				return token;

			if(response.TryGetJson<ErrorMessage>(out var error) && error.Code != 0)
			{
				if(error.Code == ErrorCodes.Busy && retries > 0)
				{
					await Task.Delay(Math.Max(500, Zongsoft.Common.Randomizer.GenerateInt32() % 2500));
					return await this.GetRemoteTicketAsync(credentialId, retries - 1);
				}

				throw new WechatException(error.Code, error.Message);
			}

			throw new WechatException(await response.Content.ReadAsStringAsync());
		}

		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		private Options.IAppSetting GetApp(string appId)
		{
			if(string.IsNullOrEmpty(appId))
				return this.Configuration.Apps.Default ?? throw new InvalidOperationException("Missing The Wechat application default configuration.");

			if(this.Configuration.Apps.TryGet(appId, out var app))
				return app;

			throw new InvalidOperationException($"The specified '{appId}' Wechat application configuration does not exist.");
		}

		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		private string GetCredentalKey(string appId)
		{
			return "Zongsoft.Wechat.Credential:" + appId.ToLowerInvariant();
		}

		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		private string GetTicketKey(string appId)
		{
			return "Zongsoft.Wechat.Ticket:" + appId.ToLowerInvariant();
		}
		#endregion

		#region 嵌套子类
		public abstract class Token
		{
			#region 构造函数
			protected Token()
			{
			}
			#endregion

			#region 公共属性
			[Zongsoft.Runtime.Serialization.SerializationMember("expires_in")]
			[System.ComponentModel.TypeConverter(typeof(ExpiryConverter))]
			public DateTime Expiry
			{
				get; set;
			}

			[Zongsoft.Runtime.Serialization.SerializationMember(Runtime.Serialization.SerializationMemberBehavior.Ignored)]
			public bool IsValid
			{
				get => this.Key != null && this.Key.Length > 0;
			}

			[Zongsoft.Runtime.Serialization.SerializationMember(Runtime.Serialization.SerializationMemberBehavior.Ignored)]
			public bool IsExpired
			{
				get => DateTime.UtcNow > this.Expiry;
			}
			#endregion

			#region 抽象属性
			[Zongsoft.Runtime.Serialization.SerializationMember(Runtime.Serialization.SerializationMemberBehavior.Ignored)]
			internal protected abstract string Key
			{
				get;
			}
			#endregion

			#region 重写方法
			public override string ToString()
			{
				return this.Key + "@" + this.Expiry.ToLocalTime().ToString();
			}
			#endregion
		}

		public class CredentialToken : Token
		{
			#region 公共字段
			[Zongsoft.Runtime.Serialization.SerializationMember("access_token")]
			public string CredentialId;
			#endregion

			#region 构造函数
			public CredentialToken(string credentialId, DateTime expiry)
			{
				this.CredentialId = credentialId;
				this.Expiry = expiry;
			}
			#endregion

			#region 重写属性
			internal protected override string Key
			{
				get => this.CredentialId;
			}
			#endregion
		}

		public class TicketToken : Token
		{
			#region 公共字段
			[Zongsoft.Runtime.Serialization.SerializationMember("ticket")]
			public string TicketId;
			#endregion

			#region 构造函数
			public TicketToken(string ticketId, DateTime expiry)
			{
				this.TicketId = ticketId;
				this.Expiry = expiry;
			}
			#endregion

			#region 重写属性
			internal protected override string Key
			{
				get => this.TicketId;
			}
			#endregion
		}
		#endregion
	}
}

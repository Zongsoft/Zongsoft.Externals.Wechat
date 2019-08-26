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
using System.Globalization;
using System.ComponentModel;
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
		private readonly Dictionary<string, CredentialToken> _localCache;
		#endregion

		#region 构造函数
		public CredentialProvider()
		{
			_http = new HttpClient();
			_localCache = new Dictionary<string, CredentialToken>();
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
		public async Task<string> GetCredentialAsync(string appId)
		{
			Options.IAppSetting app = null;

			if(string.IsNullOrEmpty(appId))
				app = this.Configuration.Apps.Default ?? throw new InvalidOperationException("Missing The Wechat application default configuration.");
			else if(!this.Configuration.Apps.TryGet(appId, out app))
				throw new InvalidOperationException($"The specified '{appId}' Wechat application configuration does not exist.");

			//首先从本地内存缓存中获取凭证标记，如果获取成功并且凭证未过期则返回该凭证号
			if(_localCache.TryGetValue(app.Id, out var token) && !token.IsExpired)
				return token.CredentialId;

			var KEY = GetCacheKey(app.Id);
			var credentialId = this.Cache.GetValue<string>(KEY);

			if(string.IsNullOrEmpty(credentialId))
			{
				token = await this.RegisterAsync(app.Id, app.Secret);

				if(this.Cache.SetValue(KEY, token.CredentialId, token.Expiration))
					_localCache[app.Id] = token;

				return token.CredentialId;
			}
			else
			{
				var expiry = this.Cache.GetExpiry(KEY);

				if(expiry.HasValue)
					_localCache[app.Id] = new CredentialToken(credentialId, DateTime.UtcNow.Add(expiry.Value));

				return credentialId;
			}
		}
		#endregion

		#region 私有方法
		private async Task<CredentialToken> RegisterAsync(string appId, string secret, int retries = 3)
		{
			var response = await _http.GetAsync($"https://api.weixin.qq.com/cgi-bin/token?grant_type=client_credential&appid={appId}&secret={secret}");

			if(response.IsSuccessStatusCode && response.TryGetJson<CredentialToken>(out var token) && token.IsValid)
				return token;

			if(response.TryGetJson<ErrorMessage>(out var error) && error.Code != 0)
			{
				if(error.Code == ErrorCodes.Busy && retries > 0)
				{
					await Task.Delay(Math.Max(500, Zongsoft.Common.Randomizer.GenerateInt32() % 2500));
					return await this.RegisterAsync(appId, secret, retries - 1);
				}

				throw new WechatException(error.Code, error.Message);
			}

			throw new WechatException(await response.Content.ReadAsStringAsync());
		}

		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		private string GetCacheKey(string appId)
		{
			return "Zongsoft.Wechat:" + appId.ToLowerInvariant();
		}
		#endregion

		#region 嵌套结构
		private struct CredentialToken
		{
			#region 公共字段
			[Zongsoft.Runtime.Serialization.SerializationMember("access_token")]
			public string CredentialId;

			[Zongsoft.Runtime.Serialization.SerializationMember("expires_in")]
			[System.ComponentModel.TypeConverter(typeof(ExpirationConverter))]
			public DateTime Expiration;
			#endregion

			#region 构造函数
			public CredentialToken(string credentialId, DateTime expiration)
			{
				this.CredentialId = credentialId;
				this.Expiration = expiration;
			}
			#endregion

			#region 公共属性
			public bool IsValid
			{
				get => this.CredentialId != null && this.CredentialId.Length > 0;
			}

			public bool IsExpired
			{
				get => DateTime.UtcNow > this.Expiration;
			}
			#endregion

			#region 重写方法
			public override string ToString()
			{
				return this.CredentialId + "@" + this.Expiration.ToLocalTime().ToString();
			}
			#endregion

			#region 类型解析
			public class ExpirationConverter : System.ComponentModel.TypeConverter
			{
				public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
				{
					return sourceType == typeof(int);
				}

				public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
				{
					return base.CanConvertTo(context, destinationType);
				}

				public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
				{
					if(value != null && value is int number)
						return DateTime.UtcNow.AddSeconds(number);

					return base.ConvertFrom(context, culture, value);
				}

				public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
				{
					return base.ConvertTo(context, culture, value, destinationType);
				}
			}
			#endregion
		}

		private struct CredentialTokenMessage
		{
			[Zongsoft.Runtime.Serialization.SerializationMember("access_token")]
			public string AccessToken;

			[Zongsoft.Runtime.Serialization.SerializationMember("expires_in")]
			public int Expires;
		}
		#endregion
	}
}

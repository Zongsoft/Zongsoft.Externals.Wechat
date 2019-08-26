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

using Zongsoft.Options;
using Zongsoft.Options.Configuration;

namespace Zongsoft.Externals.Wechat.Options.Configuration
{
	public class AppElementCollection : OptionConfigurationElementCollection<AppElement, IAppSetting>, IAppSettingCollection
	{
		#region 常量定义
		private const string XML_DEFAULT_ATTRIBUTE = "default";
		#endregion

		#region 构造函数
		public AppElementCollection() : base("app")
		{
			this.Properties.Add(new OptionConfigurationProperty(XML_DEFAULT_ATTRIBUTE, typeof(string)));
		}
		#endregion

		#region 公共属性
		public IAppSetting Default
		{
			get
			{
				var id = this.GetAttributeValue(XML_DEFAULT_ATTRIBUTE) as string;

				if(string.IsNullOrWhiteSpace(id))
					return null;

				return this[id];
			}
			set
			{
				if(value == null)
					this.SetAttributeValue(XML_DEFAULT_ATTRIBUTE, null);
				else
					this.SetAttributeValue(XML_DEFAULT_ATTRIBUTE, value.Id);
			}
		}
		#endregion

		#region 重写方法
		protected override OptionConfigurationElement CreateNewElement()
		{
			return new AppElement();
		}

		protected override string GetElementKey(OptionConfigurationElement element)
		{
			return ((AppElement)element).Id;
		}
		#endregion
	}
}

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
using System.Runtime.Serialization;

namespace Zongsoft.Externals.Wechat
{
	public class WechatException : ApplicationException
	{
		#region 构造函数
		public WechatException(string message) : base(message)
		{
		}

		public WechatException(string message, Exception innerException) : base(message, innerException)
		{
		}

		public WechatException(int code, string message) : base(message)
		{
			this.Code = code;
		}

		public WechatException(int code, string message, Exception innerException) : base(message, innerException)
		{
			this.Code = code;
		}

		protected WechatException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
			this.Code = info.GetInt32(nameof(this.Code));
		}
		#endregion

		#region 公共属性
		public int Code
		{
			get;
		}
		#endregion

		#region 重写方法
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			//添加序列化成员
			info.AddValue(nameof(this.Code), this.Code);

			//调用基类同名方法
			base.GetObjectData(info, context);
		}

		public override string ToString()
		{
			if(this.Code == 0)
				return base.ToString();
			else
				return $"[{this.Code.ToString()}] " + base.ToString();
		}
		#endregion
	}
}

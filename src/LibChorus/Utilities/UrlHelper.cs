﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Chorus.Utilities
{
	public class UrlHelper
	{
		public static string GetEscapedUrl(string url)
		{
			string xml = url;
			if (!string.IsNullOrEmpty(xml))
			{
				xml = xml.Replace("&", "&amp;");
				xml = xml.Replace("\"", "&quot;");
				xml = xml.Replace("'", "&apos;");
				xml = xml.Replace("<", "&lt;");
				xml = xml.Replace(">", "&gt;");
			}
			return xml;

		}

		public static string GetUnEscapedUrl(string attributeValue)
		{
			string url = attributeValue;
			if (!string.IsNullOrEmpty(url))
			{
				url = url.Replace("&apos;", "'");
				url = url.Replace("&quot;", "\"");
				url = url.Replace("&amp;", "&");
				url = url.Replace("&lt;", "<");
				url = url.Replace("&gt;", ">");
			}
			return url;
		}

		/// <summary>
		/// get at the value in a URL, which are listed the collection of name=value pairs after the ?
		/// </summary>
		/// <example>GetValueFromQueryStringOfRef("id", ""lift://blah.lift?id=foo") returns "foo"</example>
		public static string GetValueFromQueryStringOfRef(string url, string name, string defaultIfCannotGetIt)
		{
			if(String.IsNullOrEmpty(url))
				return defaultIfCannotGetIt;

			string originalUrl = url;
			try
			{
				Uri uri;
				url = StripSpaceOutOfHostName(url);
				if (!Uri.TryCreate(url, UriKind.Absolute, out uri) || uri == null)
				{
					throw new ApplicationException("Could not parse the url " + url);
				}
				else
				{
					//Could not parse the url lift://FTeam.lift?type=entry&label=نویس&id=e824f0ae-6d36-4c52-b30b-eb845d6c120a

					var parse = System.Web.HttpUtility.ParseQueryString(uri.Query);

					var r = parse.GetValues(name);
					var label = r == null ? defaultIfCannotGetIt : r.First();
					return string.IsNullOrEmpty(label) ? defaultIfCannotGetIt : label;
				}
			}
			catch (Exception e)
			{
#if DEBUG
				var message = String.Format("Debug mode only: GetValueFromQueryStringOfRef({0},{1}) {2}", originalUrl, name, e.Message);
				Debug.Fail(message);
				throw new ApplicationException(message,e);//vs2010 is not actually showing a failure box for me, so next hope is to throw
#endif
				return defaultIfCannotGetIt;
			}
		}

		/// <summary>
		/// this is needed because Url.TryCreate dies if there is a space in the initial part, but
		/// we're often using that part for a file name, as in "lift://XYZ Dictioanary.lift?foo=....".  Even
		/// with a %20 in place of a space, it is declared "invalid".
		/// </summary>
		/// <param name="url"></param>
		/// <returns></returns>
		private static string StripSpaceOutOfHostName(string url)
		{
			int startOfQuery = url.IndexOf('?');
			if (startOfQuery < 0)
				return url;
			string host = url.Substring(0, startOfQuery);
			string rest = url.Substring(startOfQuery, url.Length - startOfQuery);
			return host.Replace("%20", "").Replace(" ",String.Empty) + rest;
		}

		public static string GetPathOnly(string url)
		{
// DIDN"T WORK
//			Uri uri;
//				if (!Uri.TryCreate(url, UriKind.Absolute, out uri))
//				{
//					throw new ApplicationException("Could not parse the url " + url);
//				}
//
//			return uri.AbsolutePath;

			int locationOfQuestionMark = url.IndexOf('?');
			if(locationOfQuestionMark > 0)
			{
				return url.Substring(0, locationOfQuestionMark);
			}
			return url;
		}

		public static string GetUserName(string url)
		{
			Uri uri;
			if (!Uri.TryCreate(url, UriKind.Absolute, out uri))
			{
				return string.Empty;
			}
			var result = Regex.Match(uri.UserInfo, @"([^:]*)(:(.*))*");
			return result.Groups[1].Value;
		}

		public static string GetPassword(string url)
		{
			Uri uri;
			if (!Uri.TryCreate(url, UriKind.Absolute, out uri))
			{
				return string.Empty;
			}
			var result = Regex.Match(uri.UserInfo, @"([^:]*):(.*)");
			return result.Groups[2].Value;
		}

		/// <summary>
		/// gives path only, not including any query part
		/// </summary>
		public static string GetPathAfterHost(string url)
		{
			Uri uri;
			if (!Uri.TryCreate(url, UriKind.Absolute, out uri))
			{
				return string.Empty;
			}
			var s = uri.PathAndQuery;
			var i = s.IndexOf('?');
			if(i>=0)
			{
				s = s.Substring(0, i);
			}
			return s.Trim(new char[] {'/'});
		}

		public static string GetHost(string url)
		{
			Uri uri;
			if (!Uri.TryCreate(url, UriKind.Absolute, out uri))
			{
				return string.Empty;
			}
			return uri.Host;
		}
	}
}

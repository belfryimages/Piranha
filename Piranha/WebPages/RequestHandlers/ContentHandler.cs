﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

using Piranha.Models;

namespace Piranha.WebPages.RequestHandlers
{
	/// <summary>
	/// Request handler for content.
	/// </summary>
	public class ContentHandler : IRequestHandler
	{
		/// <summary>
		/// Handles the current request.
		/// </summary>
		/// <param name="context">The current context</param>
		/// <param name="args">Optional url arguments passed to the handler</param>
		public virtual void HandleRequest(HttpContext context, params string[] args) {
			HandleRequest(context, false, args) ;
		}

		/// <summary>
		/// Handles the current request.
		/// </summary>
		/// <param name="context">The current context</param>
		/// <param name="draft">Whether to get the draft or not</param>
		/// <param name="args">Optional url arguments passed to the handler</param>
		protected void HandleRequest(HttpContext context, bool draft, params string[] args) {
			if (args != null && args.Length > 0) {
				Content content = null ;
				int? width = null ;
				int? height = null ;

				try {
					// Get content by id
					content = Content.GetSingle(new Guid(args[0]), draft) ;
					if (content != null) {
						if (args.Length > 1)
							try {
								width = Convert.ToInt32(args[1]) ;
							} catch {}
						if (args.Length > 2)
							try {
								height = Convert.ToInt32(args[2]) ;
							} catch {}
					}
				} catch {
					// Get content by permalink
					var perm = Permalink.GetByName(Config.MediaNamespaceId, GetPermalink(args[0], ref width, ref height)) ;

					if (perm != null)
						content = Content.GetByPermalinkId(perm.Id) ;
				}

				// Since we don't handle viewing image drafts right now, don't execute the overhead
				// if (content.IsDraft && !Extend.ExtensionManager.Current.MediaProvider.ExistsDraft(content.Id))
				//	 content.IsDraft = false ;

				if (content != null) {
					if (height.HasValue)
						content.GetMedia(context, width, height) ;
					content.GetMedia(context, width) ;
				} else context.Response.StatusCode = 404 ;
			} else context.Response.StatusCode = 500 ;
		}

		#region Private methods
		/// <summary>
		/// Parses the media permalink and optional dimensions from the given string.
		/// </summary>
		/// <param name="str">The request string</param>
		/// <param name="width">Optional width</param>
		/// <param name="height">Optional height</param>
		/// <returns>The permalink</returns>
		private string GetPermalink(string str, ref int? width, ref int? height) {
			var segments = str.Split(new char[] { '.' }) ;

			// Get main body & possible suffix
			var body = segments[0] ;
			var suffix = segments.Length > 1 ? segments[1] : "" ;
 
			// Split the body to get optional dimensions
			var param = body.Split(new char[] { '_' }) ;
 			var name = param[0] ;
			if (param.Length > 1)
				try {
					width = Convert.ToInt32(param[1]) ;
				} catch {}
			if (param.Length > 2)
				try {
					height= Convert.ToInt32(param[2]) ;
				} catch {}

			// Return the permalink
			return name + (!String.IsNullOrEmpty(suffix) ? "." + suffix : "") ;
		}
		#endregion
	}
}

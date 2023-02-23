﻿namespace UrlShort.Models
{
	public class UrlManagement
	{
		public int Id { get; set; }
		public string Url { get; set; }
		public string ShortUrl { get; set; }
		public DateTime CreatedDateTime { get; set; } = DateTime.Now;

	}
}

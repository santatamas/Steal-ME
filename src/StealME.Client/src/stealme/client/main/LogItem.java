package stealme.client.main;

import java.util.Date;

import android.text.format.Time;

public class LogItem {
	private Time creationDate;
	private String message;
	private String title;
	
	public LogItem(Time creation, String title, String message)
	{
		this.creationDate = creation;
		this.message = message;
		this.title = title;
	}

	public Time getCreationDate() {
		return creationDate;
	}

	public String getMessage() {
		return message;
	}
	
	public String getTitle() {
		return title;
	}
}

package stealme.client.main;

import java.util.ArrayList;

import android.content.Context;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ArrayAdapter;
import android.widget.TextView;

public class LogItemAdapter extends ArrayAdapter<LogItem> {
	private ArrayList<LogItem> logItems;

	public LogItemAdapter(Context context, int textViewResourceId,
			ArrayList<LogItem> logItems) {
		super(context, textViewResourceId, logItems);
		this.logItems = logItems;
	}

	@Override
	public View getView(int position, View convertView, ViewGroup parent) {
		View v = convertView;
		if (v == null) {
			LayoutInflater vi = (LayoutInflater) this.getContext().getSystemService(Context.LAYOUT_INFLATER_SERVICE);
			v = vi.inflate(R.layout.logitem, null);
		}

		LogItem logitem = logItems.get(position);
		if (logitem != null) {
			TextView logDate = (TextView) v.findViewById(R.id.txtDate);
			TextView logMessage = (TextView) v.findViewById(R.id.txtMessage);
			TextView logTitle = (TextView) v.findViewById(R.id.txtTitle);

			if (logDate != null) {
				logDate.setText(logitem.getCreationDate().hour + ":" + logitem.getCreationDate().minute);
			}
			
			if (logMessage != null) {
				logMessage.setText(logitem.getMessage());
			}
			
			if (logTitle != null) {
				logTitle.setText(logitem.getTitle());
			}
		}
		return v;
	}
}

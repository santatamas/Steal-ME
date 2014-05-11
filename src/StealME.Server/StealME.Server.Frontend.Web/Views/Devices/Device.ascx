<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>
<%@ Import Namespace="StealME.Server.Data.DAL" %>
<%  using (Ajax.BeginForm("UpdateDevice", new { DeviceId = ((Licence)this.Model).Tracker.Id },
    new AjaxOptions { UpdateTargetId = "Device" + ((Licence)this.Model).Id, OnSuccess = "onDeviceUpdateSuccess" },
    new { id = "UpdateDeviceForm" + ((Licence)this.Model).Id }))
    {
%>
    <div id="Device<%= ((Licence)this.Model).Id %>">
        <% Html.RenderPartial("DeviceStatusControl", this.Model); %>
    </div>
<% } %>
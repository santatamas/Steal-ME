<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<StealME.Server.Frontend.Web.Models.TrackerNavigationModel>" %>
<!-- Navigation and Filters -->
<%  using (Ajax.BeginForm("UpdateNavigation","Tracking", new { DeviceId = Model.SelectedTracker.Id },
    new AjaxOptions { OnComplete = "onNavigationUpdateSuccess" },
    new { id = "NavigationForm" }))
    {
%>
<div id="Navigation">
    <%-- <%= Html.DropDownListFor(model => model.SelectedTracker.Id, 
                                 new SelectList(Model.Trackers, 
                                    "Id", 
                                    "Name", 
                                    Model.SelectedTracker.Id), 
                                 new { onchange = "$(this.form).submit();", id = "TrackerDropDown" })
        %>--%>
        
    <div id="devide-informations">
        <div id="device-name">
            Name:
            <div id="device-name-value" style="float:right;">
                <%= Model.SelectedTracker.Name %>
            </div>
        </div>
        <div id="device-status">
            State:   
            
            <div id="device-status-value" style="float:right;">
                 <span style="color:Green;">Activated</span>
            </div>          
        </div>
        <div id="device-avaliability">
            Avaliability: 
            
            <div id="device-avaliability-value" style="float:right;">
                 <span style="color:Green;">On-Line</span>
            </div>
        </div>
         <div id="device-lastupdated">
            Last Refresh: 
            
            <div id="device-lastupdated-value" style="float:right;">
                 <span style="color:Green;">---</span>
            </div>
        </div>
    </div>
    

    <div id="navigation-controls">
        <div class="navigation-control-row">
            No. displayed positions:
            <%=Html.DropDownList("No. Positions", new List<SelectListItem>
                         {
                            new SelectListItem{ Text="50", Value = "50" }, 
                            new SelectListItem{ Text="100", Value = "100" },
                            new SelectListItem{ Text="150", Value = "150" },
                            new SelectListItem{ Text="All", Value = "-1" }
                         }, new { onchange = "$(this.form).submit();", id = "NoPositionsDropDown" })%>
        </div>
        <div class="navigation-control-row">
            Date From:
            <input type="text" id="positions-from" name="positions-from" class="datefield" value="<%= DateTime.Now.AddYears(-1).ToShortDateString() %>" onchange = "$(this.form).submit();"/>
        </div>
        <div class="navigation-control-row">  
            Date To:
            <input type="text" id="positions-to" name="positions-to" class="datefield" value="<%= DateTime.Now.ToShortDateString() %>" onchange = "$(this.form).submit();"/>
        </div>
    </div>
</div>
<% } %>
<script>
    $("#NavigationForm").submit();
</script>

<!-- Activate Tracker Button -->
<% using(Ajax.BeginForm(
      "SendActivateCommand",
      "Tracking",
      new { DeviceId = Model.SelectedTracker.Id },
      new AjaxOptions { OnComplete = "onCommandTypeSuccess" }))
   {%>
            <input type="submit" class="command-button" name="submit" value="Activate Tracker" />
<% } %>

<!-- Deactivate Tracker Button -->
<% using(Ajax.BeginForm(
      "SendDeactivateCommand",
      "Tracking",
      new { DeviceId = Model.SelectedTracker.Id },
      new AjaxOptions { OnComplete = "onCommandTypeSuccess" }))
   {%>
            <input type="submit" class="command-button" name="submit" value="Deactivate Tracker" />
<% } %>

<!-- Signal Button -->
<% using(Ajax.BeginForm(
      "SendSignalCommand",
      "Tracking",
      new { DeviceId = Model.SelectedTracker.Id },
      new AjaxOptions { OnComplete = "onCommandTypeSuccess" }))
   {%>
            <input type="submit" class="command-button" name="submit" value="Signal" />
<% } %>

<!-- Enable SMS Sending Button -->
<% using(Ajax.BeginForm(
      "SendEnableSMSCommand",
      "Tracking",
      new { DeviceId = Model.SelectedTracker.Id },
      new AjaxOptions { OnComplete = "onCommandTypeSuccess" }))
   {%>
            <input type="submit" class="command-button" name="submit" value="Enable SMS" />
<% } %>

<!-- Disable SMS Sending Button -->
<% using(Ajax.BeginForm(
      "SendDisableSMSCommand",
      "Tracking",
      new { DeviceId = Model.SelectedTracker.Id },
      new AjaxOptions { OnComplete = "onCommandTypeSuccess" }))
   {%>
            <input type="submit" class="command-button" name="submit" value="Disable SMS" />
<% } %>
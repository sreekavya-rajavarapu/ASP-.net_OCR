<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="AmericanEagleDemo._Default" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <!-- for js charts -->
    <script type="text/javascript" src="https://www.google.com/jsapi"></script>
    <script type="text/javascript">
        google.load("visualization", "1", { packages: ["corechart"] });
        google.setOnLoadCallback(drawChart);
        function drawChart() {
            var options = {
                title: '',
                width: 800,
                height: 600,
                bar: { groupWidth: "95%" },
                isStacked: true
            };
            $.ajax({
                type: "POST",
                url: "Default.aspx/GetChartData",
                data: '{}',
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: function (r) {
                    var data = google.visualization.arrayToDataTable(r.d);
                    console.log(data);
                    var chart = new google.visualization.ColumnChart($("#chart")[0]);
                    chart.draw(data, options);
                },
                failure: function (r) {
                    alert(r.d);
                },
                error: function (r) {
                    alert(r.d);
                }
            });
        }
    </script>

    <div class="text-center" style="margin-top: 10px; margin-bottom: 20px;">
        <h4> Welcome, Sree kavya </h4>
        <br />
        <p> Here is the overview of your current points ...</p>
        <br />
    </div>
    <asp:GridView OnPageIndexChanging="OnPageIndexChanging" CssClass="mydatagrid" PagerStyle-CssClass="pager" HeaderStyle-CssClass="header" RowStyle-CssClass="rows"
        AutoGenerateColumns="false" AllowPaging="True" ID="pointsDGV" runat="server">
        <Columns>
            <asp:BoundField DataField="Customer Name" HeaderText="Customer Name" />
            <asp:BoundField DataField="Date" HeaderText="Date" />
            <asp:BoundField DataField="Points" HeaderText="Points" />
        </Columns>
    </asp:GridView>

    <br />
    <hr />
    <div class="text-center"><h4>Points per month</h4><div id="chart" style="display:inline-block; margin: 0 auto; height: 500px;"></div></div>

</asp:Content>

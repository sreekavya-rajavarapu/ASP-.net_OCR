<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="~/Site.Master" CodeBehind="UploadReceipt.aspx.cs" Inherits="AmericanEagleDemo.UploadReceipt" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <script>
        function showSnipper() {
            $(".loader").css({ 'opacity': 1 });
        }
    </script>
    <style>
        .loader {
          border: 16px solid #f3f3f3; /* Light grey */
          border-top: 16px solid #3498db; /* Blue */
          border-radius: 50%;
          width: 50px;
          height: 50px;
          animation: spin 2s linear infinite;
        }

        @keyframes spin {
          0% { transform: rotate(0deg); }
          100% { transform: rotate(360deg); }
        }
    </style>
    <div class="text-center">

        <h4 style="display: inline-block; margin-top: 10px;">Select a receipt to scan: </h4><asp:FileUpload id="FileUpload1" CssClass="fileupload" runat="server" > </asp:FileUpload>
        <br /><br />
        <asp:Button id="UploadButton" OnClientClick="showSnipper()" CssClass="btn btn-success" ClientIDMode="Static" Text="Upload file" OnClick="UploadButton_Click" runat="server"> </asp:Button>   
        <asp:Label id="UploadStatusLabel" runat="server"></asp:Label>
    </div>
    <br /><br />
    <div class="text-center">
        <div runat="server" id="loadingSpinner" ClientIDMode="Static" style="opacity: 0; margin: 0 auto;"  class="loader"></div>
    </div>
    

    <div class="container">
        <div class="row">
        <div class="col-sm-6">
            <asp:Image Visible="false" Width="300px" Height="500px" ID="receiptImage" runat="server" />

        </div>
        <div class="col-sm-6 text-center">
            <asp:GridView CssClass="mydatagrid" PagerStyle-CssClass="pager" HeaderStyle-CssClass="header" RowStyle-CssClass="rows"
                AutoGenerateColumns = "false" AllowPaging="True" ID="wholesaleDGV" runat="server">
                 <Columns>
                    <asp:BoundField DataField="Select" HeaderText="Select" />
                    <asp:BoundField DataField="Customer ID" HeaderText="Customer ID" />
                    <asp:BoundField DataField="Item purchased" HeaderText="Item purchased" />
                    <asp:BoundField DataField="Confidence" HeaderText="Confidence" />
                </Columns>
            </asp:GridView>
            <asp:button runat="server" OnClick="SavePoints_Click" Visible="false" type="button" Text="Save to profile" ID="savePointsBtn" CssClass="btn btn-success mybtn" />
            <br />
            <asp:Label id="savetotable" runat="server"></asp:Label>
    </div>
    </div>
    

</asp:Content>
<%@ Page Language="C#" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<script runat="server">

    public void Page_Load(object sender, EventArgs e)
    {
        // Some IIS serves manage to redirect to this page after logging in
        HttpContext.Current.Response.RedirectPermanent(Request.ApplicationPath ?? "", true);
    }

</script>

<html xmlns="http://www.w3.org/1999/xhtml">
    <head runat="server">
        <title>Title</title>
    </head>
    <body>
        <form runat="server">
        </form>
    </body>
</html>

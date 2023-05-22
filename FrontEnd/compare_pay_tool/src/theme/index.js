import { createTheme, colors } from "@material-ui/core";
import shadows from "./shadows";
import typography from "./typography";

const theme = createTheme({
  palette: {
    background: {
      dark: colors.blueGrey[900],
      default: colors.common.white,
      paper: colors.common.white,
    },
    primary: {
      main: colors.blueGrey[900],
    },
    secondary: {
      main: colors.blueGrey[900],
    },
    text: {
      primary: colors.blueGrey[900],
      secondary: colors.blueGrey[600],
    },
  },
  shadows,
  typography,
});

export default theme;

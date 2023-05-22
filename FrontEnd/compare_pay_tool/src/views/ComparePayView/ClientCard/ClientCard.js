import React from "react";
import {
  Box,
  Typography,
  Card,
  CardContent,
  Tooltip,
  Divider,
  makeStyles,
} from "@material-ui/core";
import "react-loader-spinner/dist/loader/css/react-spinner-loader.css";
import {
  ClientEnvSelect,
  ClientCatSelect,
  ClientSelect,
} from "./components/index";

const useStyles = makeStyles((theme) => ({
  root: {
    backgroundColor: theme.palette.background.dark,
    minHeight: "100%",
    paddingBottom: theme.spacing(3),
    paddingTop: theme.spacing(3),
  },
  card: {
    height: "100%",
    color: theme.palette.text.secondary,
    minHeight: 400,
  },
  formControl: {
    margin: theme.spacing(1),
    minWidth: 450,
    maxWidth: 450,
  },
  sortFormControl: {
    maxWidth: 450,
    minWidth: 450,
  },
  sortGroup: {
    position: "absolute",
    right: 0,
    top: -40,
  },
  server: {
    wordWrap: "word-break",
    textAlign: "left",
    color: "rgba(0, 0, 0, 0.54)",
  },
  serverHead: {
    color: "rgba(0, 0, 0, 0.83)",
    textAlign: "left",
    marginTop: 8,
    marginBottom: 3,
  },
}));

const ClientCard = ({
  ClientInfo,
  setClientInfo,
  valid,
  setValid,
  categoryList,
  clientList,
  setSubOpen,
}) => {
  const classes = useStyles();

  return (
    <Card className={classes.card}>
      <Box mt={2} mb={2}>
        <Tooltip title="Select Client Environment">
          <Typography variant="h5" align="center" color="primary">
            Client
          </Typography>
        </Tooltip>
      </Box>
      <Divider />
      <CardContent>
        <Box mt={2} mb={2}>
          <ClientEnvSelect
            ClientInfo={ClientInfo}
            setClientInfo={setClientInfo}
            setValid={setValid}
            setSubOpen={setSubOpen}
          />
          {ClientInfo.id === 1 ? (
            <ClientCatSelect
              ClientInfo={ClientInfo}
              setClientInfo={setClientInfo}
              categoryList={categoryList}
              setSubOpen={setSubOpen}
            />
          ) : (
            <Box></Box>
          )}
          {ClientInfo.category ? (
            <ClientSelect
              clientList={clientList}
              ClientInfo={ClientInfo}
              setClientInfo={setClientInfo}
              valid={valid}
              setSubOpen={setSubOpen}
            />
          ) : (
            <Box></Box>
          )}
        </Box>
      </CardContent>
    </Card>
  );
};

export default ClientCard;

import React from "react";
import {
  Box,
  Tooltip,
  Divider,
  Card,
  CardContent,
  Typography,
  makeStyles,
} from "@material-ui/core";

import "react-loader-spinner/dist/loader/css/react-spinner-loader.css";

import {
  ClientNamespaceSelect1,
  ClientNamespaceSelect2,
} from "../ClientCard/components";

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
  list: {
    listStyle: "none",
    textAlign: "left",
    fontSize: 14,
    paddingInlineStart: "15px",
    paddingInlineEnd: "15px",
    marginBlockStart: "5px",
    marginBlockEnd: "5px",
    wordBreak: "break-word",
  },
  listLi: {
    margin: "5px 0",
    color: "rgba(0, 0, 0, 0.54)",
  },
  listTitle: {
    color: "rgba(0, 0, 0, 0.83);",
  },
}));

const VersionsCard = ({
  ClientInfo,
  setClientInfo,
  setSubOpen,
}) => {
  const classes = useStyles();
  return (
    <Card className={classes.card}>
      <Box mt={2} mb={2}>
        <Tooltip title="Select 2 Version to be compared">
          <Typography variant="h5" align="center" color="primary">
            Versions
          </Typography>
        </Tooltip>
      </Box>
      <Divider />
      <CardContent>
        <Box mt={1} mb={2}>
          {ClientInfo.clientId > 0 &&
          ClientInfo.id === 1 &&
          ClientInfo.category ? (
            <ClientNamespaceSelect1
              ClientInfo={ClientInfo}
              setClientInfo={setClientInfo}
              setSubOpen={setSubOpen}
            />
          ) : (
            <Box></Box>
          )}
          {ClientInfo.clientId > 0 &&
          ClientInfo.id === 1 &&
          ClientInfo.category ? (
            <ClientNamespaceSelect2
              ClientInfo={ClientInfo}
              setClientInfo={setClientInfo}
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

export default VersionsCard;

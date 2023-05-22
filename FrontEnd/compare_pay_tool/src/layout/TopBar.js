import React from "react";
import clsx from "clsx";
import PropTypes from "prop-types";
import {
  AppBar,
  /*Badge,*/ Box,
  IconButton,
  Toolbar,
  makeStyles,
} from "@material-ui/core";
import MenuIcon from "@material-ui/icons/Menu";
import HelpOutlinedIcon from "@material-ui/icons/HelpOutlined";
import { Link } from "react-router-dom";

const useStyles = makeStyles(() => ({
  root: {},
  avatar: {
    width: 60,
    height: 60,
  },

  title: {
    color: "#FFFFFF",
    paddingLeft: 10,
    paddingRight: 10,
  },
}));

const TopBar = ({ className, toggleDrawer, ...rest }) => {
  const classes = useStyles();

  function wiki() {
    window.open(
      "https://wiki.dayforce.com/pages/viewpage.action?spaceKey=WFM&title=ComparePay+Tool",
      "_blank"
    );
  }

  return (
    <AppBar className={clsx(classes.root, className)} elevation={0} {...rest}>
      <Toolbar>
        <Box style={{ fontSize: "1.25rem" }}>
          <IconButton color="inherit" onClick={toggleDrawer}>
            <MenuIcon />
          </IconButton>
          <Link
            style={{ marginLeft: 8 }}
            to="/comparepaytool"
            className={classes.title}
          >
            Compare Pay Tool
          </Link>
          <Link style={{ marginLeft: 8 }} to="/" className={classes.title}>
            Home
          </Link>
          <a
            style={{ marginLeft: 8 }}
            href="http://cmtools.dayforce.com/testinfrastructure/"
            target="_blank"
            rel="noreferrer"
            className={classes.title}
          >
            Testinfrastructure
          </a>
          <a
            style={{ marginLeft: 8 }}
            href="http://cmtools2.dayforce.com/testinfrastructure/"
            target="_blank"
            rel="noreferrer"
            className={classes.title}
          >
            Testinfrastructure (Nav Cloud)
          </a>
        </Box>
        <Box flexGrow={1} />
        <IconButton color="inherit" onClick={wiki}>
          <HelpOutlinedIcon />
        </IconButton>
      </Toolbar>
    </AppBar>
  );
};

TopBar.propTypes = {
  className: PropTypes.string,
  toggleDrawer: PropTypes.func,
};

export default TopBar;

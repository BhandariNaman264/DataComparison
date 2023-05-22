import React from 'react';
import WarningIcon from '@material-ui/icons/Warning';
import { Box, Grid, Container, Card } from '@material-ui/core';
import Page from 'src/components/Page';
import { makeStyles } from '@material-ui/core';

const useStyles = makeStyles((theme) => ({
    root: {
        backgroundColor: theme.palette.background.dark,
        marginBottom: 35,
        paddingBottom: theme.spacing(72.5),
        paddingTop: theme.spacing(20)
    },
    subheader: {
        margin: '5px',
        fontSize: '16px',
    },
    card: {
        marginTop: theme.spacing(3),
        height: '100%',
        padding: theme.spacing(3),
    },
    icon: {
        fontSize: '3rem',
        color: '#ffcc00',
    },
}));

const MaintenanceRedirect = () => {
    const classes = useStyles();

    return (
        <Page
            className={classes.root}
            title='User Redirect'
        >
            <Container maxWidth='lg'>
                <Grid container spacing={3}>
                    <Grid item lg={12} md={12} xl={12} xs={12}>
                        <Card className={classes.card}>
                            <Box>
                                <h2>Compare Pay Tool currently under maintenance</h2>
                            </Box>
                            <Box mt={1}>
                                <WarningIcon className={classes.icon} />
                            </Box>
                            <Box mt={2}>
                                <h4>The client server is temporarily down due to maintenance. This is possible because the Compare Pay Tool development team is currently building or deploying a new release or bug fix on the server side.
                                When this happens, the server will be down so the client will be inaccessible at this time. Please try again in a few minutes. Thank you for assistance and we apologize for the inconvenience</h4>            
                            </Box>
                            <Box mt={2}>
                                <h4> we currently moved to new url, please use: </h4>
                                <h4><a href="https://wiki.dayforce.com/pages/viewpage.action?spaceKey=WFM&title=ComparePay+Tool" target="blank"> https://wiki.dayforce.com/pages/viewpage.action?spaceKey=WFM&title=ComparePay+Tool </a></h4>
                            </Box>
                            <Box mt={3}>
                                <h5>The Compare Pay Tool Development Team</h5>
                            </Box>
                        </Card>
                    </Grid>
                </Grid>
            </Container>
        </Page >
    );
}

export default MaintenanceRedirect; 
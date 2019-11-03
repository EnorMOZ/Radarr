import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Table from 'Components/Table/Table';
import TableBody from 'Components/Table/TableBody';
import MovieCrewRow from './MovieCrewRow';

const columns = [
  {
    name: 'poster',
    columnLabel: 'Poster',
    isVisible: true
  },
  {
    name: 'name',
    label: 'Name',
    isVisible: true
  },
  {
    name: 'department',
    label: 'Department',
    isVisible: true
  },
  {
    name: 'job',
    label: 'Job',
    isVisible: true
  }
];

class MovieCrewTableContent extends Component {

  //
  // Render

  render() {
    const {
      crew
    } = this.props;

    return (
      <div>
        <Table columns={columns}>
          <TableBody>
            {
              crew.map((item) => {
                return (
                  <MovieCrewRow
                    key={item.tmdbId}
                    {...item}
                  />
                );
              })
            }
          </TableBody>
        </Table>
      </div>
    );
  }
}

MovieCrewTableContent.propTypes = {
  crew: PropTypes.arrayOf(PropTypes.object).isRequired
};

export default MovieCrewTableContent;

import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Table from 'Components/Table/Table';
import TableBody from 'Components/Table/TableBody';
import MovieCastRow from './MovieCastRow';

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
    name: 'character',
    label: 'Character',
    isVisible: true
  }
];

class MovieCastTableContent extends Component {

  //
  // Render

  render() {
    const {
      cast
    } = this.props;

    return (
      <div>
        <Table columns={columns}>
          <TableBody>
            {
              cast.map((item) => {
                return (
                  <MovieCastRow
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

MovieCastTableContent.propTypes = {
  cast: PropTypes.arrayOf(PropTypes.object).isRequired
};

export default MovieCastTableContent;

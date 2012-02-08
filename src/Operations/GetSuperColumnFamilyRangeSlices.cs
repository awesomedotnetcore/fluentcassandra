﻿using System;
using System.Collections.Generic;
using System.Linq;
using FluentCassandra.Types;
using Apache.Cassandra;

namespace FluentCassandra.Operations
{
	public class GetSuperColumnFamilyRangeSlices : QueryableColumnFamilyOperation<FluentSuperColumnFamily>
	{
		public CassandraKeyRange KeyRange { get; private set; }

		public CassandraObject SuperColumnName { get; private set; }

		public override IEnumerable<FluentSuperColumnFamily> Execute()
		{
			var parent = new CassandraColumnParent {
				ColumnFamily = ColumnFamily.FamilyName
			};

			var output = Session.GetClient().get_range_slices(
				parent,
				SlicePredicate,
				KeyRange,
				Session.ReadConsistency
			);

			foreach (var result in output)
			{
				var r = new FluentSuperColumnFamily(result.Key, ColumnFamily.FamilyName, ColumnFamily.GetSchema(), result.Columns.Select(col => {
					var superCol = Helper.ConvertSuperColumnToFluentSuperColumn(col.Super_column);
					ColumnFamily.Context.Attach(superCol);
					superCol.MutationTracker.Clear();

					return superCol;
				}));
				ColumnFamily.Context.Attach(r);
				r.MutationTracker.Clear();

				yield return r;
			}
		}

		public GetSuperColumnFamilyRangeSlices(CassandraKeyRange keyRange, CassandraObject superColumnName, CassandraSlicePredicate columnSlicePredicate)
		{
			KeyRange = keyRange;
			SuperColumnName = superColumnName;
			SlicePredicate = columnSlicePredicate;
		}
	}
}
